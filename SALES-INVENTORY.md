# Sales Ordering × Inventory

Tài liệu mô tả luồng **Sales Order (SO) Approve**, **Delivery Note (DN) Post**, tích hợp **Inventory**, và cơ chế **concurrency** — bám sát code hiện tại trong repo.

---

## Tổng quan

```
Draft SO  ──Approve──►  Confirmed SO  (+ Inventory Reservation)
                              │
                              ▼
                        Draft DN  ──Post──►  Posted DN
                              │              (+ Fulfill + FIFO Issue)
                              │              (+ Sales Invoice)
                              ▼
                     SO: PartialDelivery / Delivered
```

| Thao tác | API | Service | Inventory workflow |
|----------|-----|---------|-------------------|
| SO Approve | `POST /api/sales-ordering/sales-orders/{id}/approve` | `SalesOrderService.ApproveAsync` | `ReserveForSalesOrderAsync` |
| DN Post | `POST /api/sales-ordering/delivery-notes/{id}/post` | `DeliveryNoteService.PostAsync` | `FulfillReservationsAsync` |
| SO Cancel (Confirmed) | `POST .../sales-orders/{id}/cancel` | `SalesOrderService.CancelAsync` | `ReleaseReservationsAsync` |
| DN Cancel (Posted) | `POST .../delivery-notes/{id}/cancel` | `DeliveryNoteService.CancelAsync` | `ReturnDeliveryIssuesAsync` |

---

## File quan trọng

| Vai trò | Path |
|---------|------|
| SO Approve / Cancel | `Backend/src/Modules/SalesOrdering/.../SalesOrders/SalesOrderService.cs` |
| DN Post / Cancel | `Backend/src/Modules/SalesOrdering/.../Deliveries/DeliveryNoteService.cs` |
| Domain SO / DN | `.../Domain/Entities/SalesOrder.cs`, `DeliveryNote.cs`, `SalesOrderLine.cs` |
| Inventory workflow | `Backend/src/Modules/Inventory/.../Workflows/InventorySalesReservationWorkflowService.cs` |
| Availability check | `.../Inventory/Internal/InventoryAvailabilityChecker.cs` |
| FIFO issue | `.../Inventory/Internal/InventoryFifoIssuer.cs` |
| Transaction + retry | `.../SalesOrdering.Infrastructure/.../SalesOrderingUnitOfWork.cs` |
| UPDLOCK balance | `.../Inventory.Infrastructure/.../InventoryBalanceRepository.cs` |
| Exception → HTTP | `Backend/src/Api/PrismERP.Api/Middlewares/GlobalExceptionHandler.cs` |
| FE SO | `Frontend/prism-erp-web/src/pages/salesOrdering/hooks/useSalesOrderForm.ts` |
| FE DN | `Frontend/prism-erp-web/src/pages/salesOrdering/delivery/useDeliveryNoteFromSO.ts` |

---

## Mô hình Inventory

### Aggregate chính

| Entity | Mục đích |
|--------|----------|
| **InventoryBalance** | Tồn theo `(ProductId, WarehouseId)`: `QuantityOnHand`, `ReorderLevel` |
| **InventoryCostLayer** | Lớp giá FIFO — nhận từ GR, tiêu thụ khi issue |
| **InventoryReservation** | Giữ chỗ tồn cho SO line; `ReferenceId` = **SalesOrderLineId** |
| **InventoryMovement** | Audit Receipt / Issue / Return |

### Công thức available

```
available = QuantityOnHand − SUM(Active reservation RemainingQuantity)
```

`InventoryAvailabilityChecker.EnsureAvailablesAsync` tính trên snapshot DB tại thời điểm gọi.

### Reservation lifecycle

| Trạng thái | Ý nghĩa |
|------------|---------|
| **Active** | Đang giữ chỗ; có thể fulfill từng phần |
| **Fulfilled** | `FulfilledQuantity >= Quantity` |
| **Released** | SO Cancel — bỏ giữ chỗ |

Quy ước: **1 SO line = tối đa 1 reservation Active** (`ReferenceType = SalesOrder`, `ReferenceId = SalesOrderLineId`).

### Service split

| Service | Vai trò |
|---------|---------|
| `InventorySalesReservationWorkflowService` | Reserve / Fulfill / Release / Return — **không** gọi `SaveChanges` |
| `InventoryReservationAdminService` | API inventory trực tiếp — save ngay |
| `InventoryReceivingWorkflowService` | GR Post — nhập kho (Purchasing) |

---

## Trạng thái Sales Order

```
Draft ──Approve──► Confirmed ──DN Post (partial)──► PartialDelivery ──DN Post (full)──► Delivered
  │                    │
  └── Cancel ──────────┴── Cancel (nếu chưa DN Posted, chưa giao) ──► Cancelled
```

| Status | Cho phép |
|--------|----------|
| **Draft** | Sửa lines, Approve, Cancel |
| **Confirmed** | Tạo DN Draft, Cancel (release reservation) |
| **PartialDelivery** | Tạo / Post DN tiếp |
| **Delivered** | Xem DN đã post; không Cancel SO |
| **Cancelled** | Terminal |

SO **Approve** trong domain = chuyển `Draft → Confirmed` (không có status riêng tên "Approved").

---

## Trạng thái Delivery Note

```
Draft ──Post──► Posted ──Cancel──► Cancelled
```

| Status | Cho phép |
|--------|----------|
| **Draft** | Sửa qty, Post |
| **Posted** | Cancel (reverse inventory + rollback SO qty delivered) |
| **Cancelled** | Terminal |

DN chỉ tạo/sửa khi SO ở **Confirmed** hoặc **PartialDelivery**.

---

## Luồng SO Approve

**Entry:** `SalesOrderService.ApproveAsync` → `ExecuteInTransactionWithRetryAsync`

### Các bước (trong transaction)

1. Reload SO + lines (`GetByIdWithLinesForUpdateAsync`)
2. Guard: `Status != Draft` → `ConflictException` (409, không retry)
3. `ReserveForSalesOrderAsync` cho từng line:
   - Load balance `(ProductId, WarehouseId)` — **phải tồn tại sẵn**
   - `PermisticLockingByBalanceIdsAsync` — UPDLOCK + ORDER BY Id (tránh deadlock)
   - Chặn reservation Active trùng `ReferenceId`
   - `EnsureAvailablesAsync`: `QuantityOrdered ≤ available`
   - INSERT `InventoryReservation` (full `QuantityOrdered`)
4. `salesOrder.Approve()` → `Confirmed`
5. `SaveChangesAsync` — commit transaction

### Warehouse

`warehouseId = 1` hardcode trong `ApproveAsync`. TODO: gán kho theo sản phẩm hoặc SO.

### Lỗi thường gặp

| Lỗi | HTTP | Nguyên nhân |
|-----|------|-------------|
| Balance không tồn tại | 422 | Product chưa có tồn ở warehouse 1 |
| Insufficient available quantity | 422 | Tồn − reserved < qty ordered |
| SO already Confirmed | 409 | Double-approve |

---

## Luồng DN Post

**Entry:** `DeliveryNoteService.PostAsync` → `ExecuteInTransactionWithRetryAsync`

### Các bước (trong transaction)

1. Reload DN + SO
2. Guard: `DN.Status != Draft` → 409
3. Load reservations **trong transaction** (`GetActivesByReferencesAsync` theo `SalesOrderLineId`)
4. Với mỗi DN line:
   - Phải có reservation Active
   - `QuantityDelivered ≤ reservation.RemainingQuantity`
   - Build `FulfillReservationLine` + `CreateInvoiceLineRequest`
5. `FulfillReservationsAsync`:
   - `RecordFulfillment` trên reservation
   - FIFO `IssueFromBalance` — giảm on-hand, cost layers, tạo Issue movements
6. `deliveryNote.Post(salesOrder)` — cập nhật `QuantityDelivered`, SO delivery status
7. `invoiceService.CreateAsync` — Sales Invoice (gọi `SaveChanges` nội bộ, cùng scoped `PrismERPDbContext` trong transaction)
8. `salesOrder.UpdateInvoiceStatus()`
9. `SaveChangesAsync`

### Partial delivery

- Reservation giữ **full qty ordered** khi Approve
- Mỗi DN Post fulfill một phần → `RemainingQuantity` giảm dần
- Nhiều DN Draft trên cùng SO được phép (backend không chặn)
- SO status: `PartialDelivery` → `Delivered` khi mọi line `QuantityDelivered == QuantityOrdered`

### Lỗi thường gặp

| Lỗi | HTTP | Nguyên nhân |
|-----|------|-------------|
| No active reservation | 422 | SO chưa Approve hoặc reservation đã release |
| Delivery qty > remaining reservation | 422 | Partial delivery vượt phần còn giữ |
| DN already Posted | 409 | Double-post |

---

## Luồng Cancel

### SO Cancel (Confirmed)

Trong `ExecuteInTransactionAsync` (không retry):

1. Nếu có DN **Posted** → reject
2. Xóa mọi DN **Draft**
3. Reject nếu bất kỳ line có `QuantityDelivered > 0`
4. `ReleaseReservationsAsync` — mọi SO line phải có reservation Active tương ứng
5. `salesOrder.Cancel()`

Draft SO Cancel: không đụng inventory.

### DN Cancel (Posted)

1. Load DN + SO (ngoài transaction)
2. Trong transaction: `ReturnDeliveryIssuesAsync` → reverse fulfillment, restore layers/balance, movement Return
3. `deliveryNote.Cancel(salesOrder)` — rollback `QuantityDelivered`

---

## Transaction & SaveChanges

Tất cả module dùng **một** scoped `PrismERPDbContext` → transaction Sales bọc được thay đổi Inventory và Finance.

| Ngữ cảnh | Transaction | SaveChanges |
|----------|-------------|-------------|
| SO Approve | `ExecuteInTransactionWithRetryAsync` | 1 lần cuối (Sales UoW) |
| DN Post | `ExecuteInTransactionWithRetryAsync` | Invoice service + Sales UoW (cùng TX) |
| SO/DN Cancel | `ExecuteInTransactionAsync` | 1 lần cuối |
| Inventory workflow | — | **Không** save |

Fail bất kỳ bước nào → rollback toàn bộ.

---

## Concurrency

### Cơ chế

| Layer | Cách hoạt động |
|-------|----------------|
| **Optimistic** | Mọi `Entity` có `RowVersion` (SQL Server `rowversion`) |
| **UPDLOCK (Reserve)** | `PermisticLockingByBalanceIdsAsync` — serialize reserve cùng balance |
| **Status guard** | Approve/Post reload trong TX; status đã đổi → `ConflictException` (409, no retry) |
| **Retry** | `ExecuteInTransactionWithRetryAsync` — tối đa 3 lần khi `DbUpdateConcurrencyException` |
| **Fresh read** | `ChangeTracker.Clear()` trước mỗi retry; reservation load **trong** TX khi Post |

Isolation mặc định: **READ COMMITTED**.

### Kịch bản

| Kịch bản | Kết quả |
|----------|---------|
| 2 user Approve **cùng SO** | Commit đầu thắng; commit sau → 409 "already Confirmed" |
| 2 SO khác nhau, cùng product/warehouse | UPDLOCK serialize; SO thứ hai đọc reserved qty đúng |
| 2 user Post **cùng DN** | 409 "already Posted" |
| 2 DN khác nhau, cùng SO line (partial) | `RowVersion` conflict → retry → validate `RemainingQuantity` mới → OK hoặc 422 |
| Retry hết 3 lần | 409 "Data was modified by another user..." |

### Exception → HTTP

| Exception | HTTP | Ghi chú |
|-----------|------|---------|
| `ConflictException` | 409 | Double-approve/post, retry exhausted |
| `DbUpdateConcurrencyException` | 409 | (handler) hoặc retry trước khi tới handler |
| `BusinessException` | 422 | Thiếu tồn, qty vượt reservation, rule nghiệp vụ |
| `NotFoundException` | 404 | |
| `ValidationException` | 400 | |
| `OperationCanceledException` | — | Client disconnect; log Information, không response |

### Decision tree

```
Exception trong Approve / Post
├── ConflictException (409)
│   ├── Cùng SO approve 2 lần
│   └── Cùng DN post 2 lần
├── BusinessException (422)
│   └── Qty / tồn / reservation sau reload
├── DbUpdateConcurrencyException
│   ├── attempt < 3 → Clear tracker + retry
│   └── attempt = 3 → ConflictException (409)
└── OperationCanceledException → bỏ qua (client disconnect)
```

### Chưa áp dụng UPDLOCK khi Fulfill

`FulfillReservationsAsync` dùng `GetByIdsForUpdateAsync` (EF tracking, không UPDLOCK). An toàn nhờ validate qty + `RowVersion` + retry. Có thể bổ sung UPDLOCK reservation/balance nếu cần giảm retry.

---

## Frontend

| Màn | Hành vi |
|-----|---------|
| SO modal Approve | Save draft trước → gọi approve API |
| SO list Approve | Gọi approve trực tiếp |
| DN Post | **Chỉ gọi post API** — không auto-save qty đã sửa trên UI |

**Lưu ý UX:** User sửa qty trên DN Draft rồi bấm **Post** mà chưa **Save** → backend post qty cũ trên DB. Nên save trước post hoặc gộp save+post.

409/422 từ API được hiển thị qua `parseApiError` / `getToastMessage`.

---

## Known limitations & TODO

| # | Mục | Trạng thái |
|---|-----|------------|
| 1 | `warehouseId = 1` hardcode khi Approve | TODO |
| 2 | SO order number prefix `PO-` (copy từ Purchase Order) | Bug nhỏ |
| 3 | DN Post không auto-save trên FE | UX gap |
| 4 | Không có integration test Approve/Post | TODO |
| 5 | API chưa gắn permission `salesorder:approve` | TODO |
| 6 | `ReturnDeliveryIssuesAsync` comment "fix later" | Hoạt động nhưng chưa hardening concurrency |

---

## Checklist test thủ công

1. **Happy path:** Tạo SO Draft → Approve (đủ tồn WH 1) → DN Draft → Save → Post → kiểm tra tồn giảm, invoice tạo, SO status
2. **Thiếu tồn:** Approve SO vượt available → 422
3. **Partial delivery:** 2 DN cho 1 SO, tổng qty = ordered
4. **Double Approve / Post:** 2 tab → expect 409
5. **SO Cancel Confirmed:** Có reservation, không DN Posted → reservation Released
6. **DN Cancel Posted:** Tồn restore, SO qty delivered rollback

---

## Liên quan module khác

| Module | Luồng inventory |
|--------|-----------------|
| **Purchasing** | GR Post → `ReceiveStockAsync` (nhập kho, tạo cost layer) |
| **Finance** | DN Post → Sales Invoice tự động |

Chi tiết Purchasing/GR: xem `GoodsReceiptService.PostAsync` và `InventoryReceivingWorkflowService`.
