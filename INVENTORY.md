# Inventory Module — Nghiệp vụ & xử lý kỹ thuật

Tài liệu mô tả luồng tồn kho, các service đã tách, quy ước transaction và các vấn đề cần lưu ý.

## Mô hình dữ liệu

| Entity | Vai trò |
|--------|---------|
| **InventoryBalance** | Tồn vật lý theo `Product × Warehouse` (`QuantityOnHand`) |
| **InventoryCostLayer** | Lớp giá vốn FIFO (nhập tạo layer, xuất consume layer) |
| **InventoryReservation** | Giữ chỗ (soft lock) — **chưa** trừ on-hand |
| **InventoryMovement** | Sổ cái: Receipt / Issue / Return / Adjustment |

**Available** khi reserve:

```
Available = QuantityOnHand - SUM(Active reservation RemainingQuantity)
```

`RemainingQuantity = Quantity - FulfilledQuantity` (reservation vẫn Active khi partial fulfill).

---

## Cấu trúc Application layer (đã tách)

```
Inventory/
  Queries/              → IInventoryQueryService          (API GET, read-only)
  Admin/                → Controller + seeder (có SaveChanges)
    IInventoryBalanceAdminService      EnsureBalance
    IInventoryManualStockAdminService Receive, Issue, Adjust
    IInventoryReservationAdminService Reserve, Release (API thủ công)
  Workflows/            → SO / DN / GR (không SaveChanges — caller save)
    IInventorySalesReservationWorkflowService
    IInventoryReceivingWorkflowService
  Internal/             → BalanceAccess, AvailabilityChecker, FifoIssuer
  Mapping/InventoryDtoMapper.cs
```

### Ai gọi service nào

| Luồng | Service |
|-------|---------|
| API GET balances/movements/... | `IInventoryQueryService` |
| POST `/balances` (setup) | `IInventoryBalanceAdminService` |
| POST receive/issue/adjust (API) | `IInventoryManualStockAdminService` |
| POST reserve/release (API) | `IInventoryReservationAdminService` |
| **SO Approve** | `ReserveForSalesOrderAsync` (workflow) |
| **DN Post / Cancel** | `FulfillReservationsAsync` / `ReturnDeliveryIssuesAsync` |
| **GR Post** | `ReceiveStockAsync` (workflow) |

### Quy ước SaveChanges

| Ngữ cảnh | Ai SaveChanges |
|----------|----------------|
| SO Approve, DN Post, GR Post (trong `ExecuteInTransactionAsync`) | Module Sales/Purchasing — **một lần cuối** |
| API inventory trực tiếp | Admin service — save ngay sau thao tác |
| Workflow methods | **Không** save |

Cùng `PrismERPDbContext` scoped → transaction SO/GR bọc được thay đổi inventory. Fail bất kỳ bước nào → **rollback toàn bộ**.

---

## Luồng Sales

### 1. SO Approve → Reserve

1. Với mỗi SO line: load balance (phải **đã tồn tại**), check không trùng reservation Active cho cùng `ReferenceId` (= **SalesOrderLineId**)
2. `EnsureAvailable`: `requested ≤ onHand - reserved`
3. Tạo `InventoryReservation` (full `QuantityOrdered`)
4. SO → Confirmed

**Chưa làm:** SO Cancel chưa `ReleaseReservation`. `warehouseId` hardcode = 1 trong Approve.

### 2. DN Post → Fulfill + Issue

Trong transaction:

1. `FulfillReservationsAsync`: `RecordFulfillment` + FIFO issue (`IssueFromBalance`)
2. `deliveryNote.Post(salesOrder)` — cập nhật qty delivered, SO status
3. Tạo Sales Invoice (Finance)
4. `SaveChanges`

Partial delivery: reservation Active, `RemainingQuantity` giảm dần qua nhiều DN.

### 3. DN Cancel (Posted) → Return

`ReturnDeliveryIssuesAsync`: reverse fulfillment, restore layer/balance, movement Return — đối chiếu issue movements theo `deliveryNumber`.

---

## Luồng Purchasing

### GR Post → Receive

`ReceiveStockAsync` (workflow): get/create balance (track only), tạo cost layer, tăng on-hand, movement Receipt. **Không** SaveChanges trong workflow — GR transaction save một lần.

---

## Thao tác thủ công (API)

| API | Mục đích |
|-----|----------|
| **Receive** | Nhập kho có nguồn (GR hoặc manual) |
| **Issue** | Xuất tay theo số lượng |
| **Adjust** | **Kiểm kê / cân sổ** — set `NewQuantity` (delta so với on-hand). Tăng: tạo layer + unit cost. Giảm: FIFO issue |

Adjust **không** sửa reservation — cẩn thận khi giảm tồn đã có SO reserve.

---

## Concurrency & lỗi

| Loại | Xử lý đề xuất |
|------|----------------|
| **Business rule** (Draft only, thiếu tồn, qty vượt reservation) | `BusinessException` → HTTP 422 |
| **Cùng DN, 2 user Post** | Reload → status Posted → 409, **không retry** |
| **2 DN khác, cùng balance** | RowVersion conflict → retry transaction 2–3 lần (chưa implement) |
| **Hết hàng thật** | 422 sau reload — **không retry mù** |

`RowVersion` trên mọi entity — `DbUpdateConcurrencyException` hiện rơi vào 500 (nên map 409 sau).

`EnsureAvailableAsync` đọc no-tracking — có thể stale; RowVersion lúc save là lớp bảo vệ cuối.

---

## FIFO

`FifoCosting.Consume` — layer cũ trước. Không đủ layer → `"Insufficient inventory in cost layers for FIFO issue."` (khác message thiếu available lúc reserve).

---

## API endpoints (tóm tắt)

| Method | Path | Service |
|--------|------|---------|
| GET | `/api/inventory/balances` | Query |
| POST | `/api/inventory/balances` | BalanceAdmin |
| POST | `/api/inventory/receive` | ManualStockAdmin |
| POST | `/api/inventory/issue` | ManualStockAdmin |
| POST | `/api/inventory/adjust` | ManualStockAdmin |
| POST | `/api/inventory/reservations` | ReservationAdmin |
| POST | `/api/inventory/reservations/{id}/release` | ReservationAdmin |

---

## Gap / TODO kỹ thuật

- [ ] SO Cancel → release reservation
- [ ] SO amend line qty → adjust reservation
- [ ] Warehouse từ SO/PO thay vì hardcode
- [ ] Concurrency: 409 + retry policy trên `ExecuteInTransactionAsync`
- [ ] Query cost layers dùng read-only (hiện dùng ForUpdate trong query)
- [ ] Unique index `(ProductId, WarehouseId)` trên balance — tránh race create
