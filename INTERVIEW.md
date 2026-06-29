# PrismERP — Interview Talking Points

Tài liệu này hướng dẫn cách **trình bày project PrismERP khi phỏng vấn** theo thứ tự từ tổng quan đến chi tiết kỹ thuật.
Mỗi mục có **điểm nói** + **câu hỏi kỳ vọng** + **câu trả lời gợi ý**.

---

## 1. Mở đầu — Pitch 30 giây

> "PrismERP là một **modular monolith ERP mini** xây bằng .NET 9 và React. Backend chia làm 5 module độc lập — Identity, Inventory, Purchasing, Sales, Finance — chia sẻ một SQL Server database nhưng mỗi module có schema riêng. Mình xây dự án này để học domain-driven design, transaction với concurrency, và authorization trong thực tế, không phải tutorial CRUD."

**Tại sao modular monolith, không microservices?**
> "Với team nhỏ và giai đoạn học, monolith cho phép gọi service cross-module trong cùng transaction mà không cần distributed transaction hay outbox. Khi cần tách sau, boundary module đã rõ ràng vì mỗi module có DbContext config, UnitOfWork, và service riêng."

---

## 2. Kiến trúc tổng thể

**Nói theo luồng:**

```
Client (React + Redux)
  ↓ JWT Bearer
ASP.NET Core API
  ↓ Middleware: Blacklist check → Permission hydration
  ↓ [Authorize] + [RequirePermission("...")]
Controller
  ↓ Service (use case)
  ↓ UnitOfWork → Repository → EF Core
SQL Server (schema per module)
```

**Điểm nhấn:**
- Controller chỉ nhận request, gọi service, trả response — không có logic
- Service orchestrate use case: load entity, gọi domain method, gọi workflow cross-module, SaveChanges
- Domain entity giữ invariant: `SalesOrder.Approve()` throw nếu không phải Draft
- UnitOfWork bọc transaction + retry khi concurrency exception

**Câu hỏi hay được hỏi:**
> "Clean Architecture khác Repository + Service bình thường chỗ nào?"

> "Em tách Domain / Application / Infrastructure để dependency chỉ đi một chiều — Application không biết EF Core, Infrastructure implement interface của Application. Nhưng em không dùng CQRS/MediatR vì chưa thấy project đủ phức tạp để justify."

---

## 3. Domain-Driven Design — Entity và Business Logic

**Trình bày qua ví dụ SalesOrder:**

```csharp
// Không để logic ở service, gọi method trên entity
salesOrder.Approve();         // throw nếu không phải Draft
salesOrder.UpdateDeliveryStatus(); // tính từ lines, không nhận param từ ngoài
```

- Entity là **Aggregate Root**: `SalesOrder`, `PurchaseOrder`, `InventoryBalance`
- Invariant bảo vệ trong entity, không ở service
- Service chỉ orchestrate: load → validate cross-module → gọi domain method → save

**Câu hỏi:**
> "Aggregate root là gì, tại sao cần?"

> "Aggregate root là entry point duy nhất để modify aggregate. Ví dụ để thêm line vào SO phải gọi `salesOrder.AddLine(...)`, không được tạo `SalesOrderLine` độc lập rồi add vào DB trực tiếp. Điều này đảm bảo business rule như 'không duplicate product' được enforce mọi lúc."

---

## 4. Flow Purchasing — PO → GR → Inventory

**Trace từ đầu đến cuối:**

1. **Tạo PO Draft** → `PurchaseOrder.CreateDraft(...)`, validate supplier/warehouse tồn tại
2. **Approve PO** → `PO.Approve()` → status `Approved`, domain method check Draft và có lines
3. **Tạo GR Draft** → link `PurchaseOrderId`, validate qty không vượt remaining trên PO
4. **Post GR** → trong 1 transaction:
   - Load GR + PO with UPDLOCK
   - `InventoryReceivingWorkflowService.ReceiveStockAsync`: upsert `InventoryBalance`, tạo `InventoryCostLayer` (FIFO), tạo `InventoryMovement`
   - `GR.MarkPosted()`, `PO.RecordReceipt(...)`
   - `SaveChanges`
5. **Cancel GR** → reverse: restore cost layer, decrease balance, create Return movement, reset PO receive qty

**Điểm nhấn:** Inventory nhận stock qua workflow service, không phải gọi trực tiếp từ GR service. Separation of concerns.

---

## 5. Flow Sales — SO → DN → Invoice

1. **Approve SO** → `InventorySalesReservationWorkflowService.ReserveForSalesOrderAsync`:
   - UPDLOCK trên `InventoryBalance` theo thứ tự id ascending (tránh deadlock)
   - Check available = `OnHand − SUM(active reservations)`
   - Tạo `InventoryReservation` (Active)
2. **Post DN** → `FulfillReservationsAsync`:
   - Update reservation `FulfilledQuantity`
   - FIFO issue: tiêu thụ cost layer theo thứ tự nhận trước
   - Tạo `InventoryMovement` (Issue)
   - Tạo Sales Invoice tự động
3. **Cancel DN** → `ReturnDeliveryIssuesAsync`: restore cost layer + balance, reverse reservation
4. **Cancel SO (Confirmed)** → `ReleaseReservationsAsync`: release tất cả active reservation

---

## 6. Concurrency — Cách xử lý và test

**Vấn đề:** Hai người cùng approve SO có stock = 10, mỗi người order 8. Nếu không lock → oversell.

**Giải pháp:**

```csharp
// 1. UPDLOCK trên InventoryBalance — serialize concurrent reserves
await unitOfWork.Balances.PermisticLockingByBalanceIdsAsync(balanceIds, ct);

// 2. Transaction + retry khi DbUpdateConcurrencyException
await unitOfWork.ExecuteInTransactionWithRetryAsync(async ct => {
    db.ChangeTracker.Clear(); // reload fresh mỗi retry
    // ... logic ...
}, cancellationToken);
```

- Order lock theo balance ID ascending → tránh deadlock giữa SO1(A,B) và SO2(B,A)
- Max 3 retries, `ConflictException` (đã approve rồi) không retry

**Integration tests:**

```csharp
// Chạy 2 approve song song, dùng Barrier để sync
await RunConcurrently(
    () => ApproveAsync(so1.Id),  // qty 8 vs stock 10
    () => ApproveAsync(so2.Id)); // chỉ 1 thành công

Assert.Equal(10m, balance.QuantityOnHand - totalReserved); // không oversell
```

**18 integration tests** chạy trên SQL Server thật (không in-memory).

---

## 7. Authorization — Policy-Based

**Luồng:**

```
JWT token (chứa roles)
  ↓
PermissionsEnrichmentMiddleware
  → query DB: User → Role → Permission codes
  → ghi vào HttpContext.Items["permissions"]
  ↓
[Authorize] + [RequirePermission("salesorder:approve")]
  ↓
PermissionAuthorizationHandler
  → đọc Items["permissions"]
  → check contains "salesorder:approve"
  → Succeed / 403
```

**Tại sao không nhét permissions vào JWT?**
> "JWT phình to khi user có nhiều permission. Hơn nữa, nếu revoke permission thì JWT cũ vẫn còn hiệu lực cho đến khi expire. Load per-request từ DB đảm bảo luôn fresh, và cũng đã có middleware cache vào Items nên không query nhiều lần."

**Role mapping:**

| Role | Quyền |
|------|-------|
| staff | Read + tạo/post GR/DN |
| leader | staff + approve SO |
| manager | full operational (approve/cancel mọi thứ) |
| super_admin | all + bypass `*` |

---

## 8. Authentication — JWT + Refresh Token

- **Access token** (30 phút): lưu trong Redux memory, không localStorage
- **Refresh token** (7 ngày): HttpOnly cookie, không accessible từ JS
- **F5 restore**: `POST /refresh-page` đọc cookie → trả access token mới. Không rotate refresh khi F5 (tránh `DbUpdateConcurrencyException` khi React StrictMode mount 2 lần)
- **Blacklist**: bảng `RefreshTokens` có `IsActive`. `BlockUserBlacklistMiddleware` check mỗi request

---

## 9. Inventory — FIFO Cost Layer

**Khi nhận hàng (GR Post):**
- Tạo `InventoryCostLayer` với `UnitCost` và `AvailableQuantity`
- Tăng `InventoryBalance.QuantityOnHand`

**Khi xuất hàng (DN Post):**
- Lấy layers theo `ReceivedAt` ascending (FIFO)
- Tiêu thụ từng layer cho đến đủ qty
- Tạo `InventoryMovement` per layer với UnitCost tại thời điểm nhập

**Khi hủy:**
- Tìm lại movement đã tạo, restore layer và balance

---

## 10. Những thứ chủ động nhắc khi phỏng vấn biết để tránh bị hỏi sâu

| Chủ đề | Nói trước |
|--------|-----------|
| Không có microservices | "Modular monolith là phù hợp cho stage này, tách sau khi có nhu cầu thật" |
| Không có unit test | "Hiện có 18 integration tests full-flow; unit test domain thuần là next step" |
| `warehouseId = 1` hardcode | "Simplification — production sẽ cho user chọn warehouse theo branch" |
| Không có CI pipeline | "Next step thêm GitHub Actions chạy integration tests" |
| Domain events chưa dispatch | "Đã có `RaiseDomainEvent` infrastructure; events sẽ dispatch khi cần real-time push qua SignalR" |

---

## 11. Câu hỏi hay bị hỏi và câu trả lời nhanh

**"Transaction boundary ở đâu?"**
> Service gọi `ExecuteInTransactionAsync` hoặc `ExecuteInTransactionWithRetryAsync`. UnitOfWork bọc toàn bộ use case trong 1 transaction. Cross-module (SO + Inventory) chạy chung transaction vì cùng DbContext.

**"N+1 query có xảy ra không?"**
> Dùng `Include()` khi cần eager load lines, và `AsNoTracking()` cho read-only queries. Không dùng lazy loading.

**"Concurrency token?"**
> Hiện dùng UPDLOCK + retry. Có thể thêm `RowVersion` concurrency token cho optimistic concurrency trên entity nếu cần.

**"Redis dùng để làm gì?"**
> Infrastructure đã setup `ICacheService`. Hiện chưa cache gì vì chưa identify hot query. Redis sẵn sàng khi cần, ví dụ cache catalog products.

**"Tại sao không dùng CQRS?"**
> Chưa thấy benefit đủ lớn để justify complexity thêm. Service hiện tại đã tách Read (AsNoTracking, DTO) và Write (tracking, domain method, SaveChanges).
