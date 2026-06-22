# Inventory Concurrency — Implementation Log

> Related analysis: [INVENTORY-CONCURRENCY.md](INVENTORY-CONCURRENCY.md)

This file records every code change made to implement the concurrency strategies described in the analysis.

---

## Overview of what was implemented

| Concern | Strategy | Status |
|---|---|---|
| Double-approve same SO | Status guard inside TX → `ConflictException` (409) | ✅ |
| Two SOs over-reserving same product/warehouse | UPDLOCK on `InventoryBalance` row during Reserve | ✅ |
| Double-post same DN | Status guard inside TX → `ConflictException` (409) | ✅ |
| Two DNs competing for same balance/reservation | `DbUpdateConcurrencyException` → retry ×3 with fresh reload | ✅ |
| Stale reservation reads in Post | Reservations loaded **inside** the transaction | ✅ |
| `DbUpdateConcurrencyException` surfaced as 500 | Mapped to 409 in `GlobalExceptionHandler` | ✅ |
| `OperationCanceledException` logged as Error | Now logged at `Information`, no response body | ✅ |

---

## Files changed

### 1. `BuildingBlocks.Domain/Exceptions/ConflictException.cs` *(new)*

```
Backend/src/BuildingBlocks/PrismERP.BuildingBlocks.Domain/Exceptions/ConflictException.cs
```

A new domain exception for explicit business conflicts (e.g. "document already processed").
Maps to **HTTP 409 Conflict** in the global handler.

```csharp
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
```

---

### 2. `GlobalExceptionHandler.cs`

```
Backend/src/Api/PrismERP.Api/Middlewares/GlobalExceptionHandler.cs
```

Three new cases added to the exception switch:

| Exception | HTTP | Type |
|---|---|---|
| `ConflictException` | 409 | `conflict` |
| `DbUpdateConcurrencyException` | 409 | `conflict` — "Data was modified by another user. Please refresh and try again." |
| `OperationCanceledException` / `TaskCanceledException` | no body | logged at `LogInformation`, returns `true` without writing a response |

`OperationCanceledException` is common when the browser navigates away or F5s mid-request. Logging it at Error level was noise; it is now at Information.

---

### 3. `ISalesOrderingUnitOfWork.cs` + `SalesOrderingUnitOfWork.cs`

```
Backend/src/Modules/SalesOrdering/PrismERP.Modules.SalesOrdering.Application/Abtractions/ISalesOrderingUnitOfWork.cs
Backend/src/Modules/SalesOrdering/PrismERP.Modules.SalesOrdering.Infrastructure/Persistence/SalesOrderingUnitOfWork.cs
```

New method **`ExecuteInTransactionWithRetryAsync`**:

```csharp
Task ExecuteInTransactionWithRetryAsync(
    Func<CancellationToken, Task> action,
    CancellationToken cancellationToken = default);
```

Implementation behaviour:

1. Before each attempt, calls `db.ChangeTracker.Clear()` so that EF's identity cache is empty and every entity load inside the lambda hits the database rather than a stale in-memory snapshot.
2. Wraps the lambda in a `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync` block.
3. On `DbUpdateConcurrencyException`: rolls back and retries (up to **3 attempts**).
4. On any other exception (`ConflictException`, `BusinessException`, etc.): rolls back and re-throws immediately — **no retry**.
5. If all 3 attempts fail with `DbUpdateConcurrencyException`, throws `ConflictException` → 409.

---

### 4. `IInventoryBalanceRepository.cs` + `InventoryBalanceRepository.cs`

```
Backend/src/Modules/Inventory/PrismERP.Modules.Inventory.Application/Abstractions/Persistence/IInventoryBalanceRepository.cs
Backend/src/Modules/Inventory/PrismERP.Modules.Inventory.Infrastructure/Persistence/Repositories/InventoryBalanceRepository.cs
```

New method **`GetByIdForUpdateWithLockAsync`**:

```csharp
Task<InventoryBalance?> GetByIdForUpdateWithLockAsync(int id, CancellationToken cancellationToken = default);
```

Implementation uses a raw-SQL query hint to acquire an **UPDLOCK + ROWLOCK** on the balance row:

```csharp
db.InventoryBalances
    .FromSqlInterpolated(
        $"SELECT * FROM inventory.InventoryBalances WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
    .FirstOrDefaultAsync(cancellationToken);
```

**Why UPDLOCK?**  
Without it, two concurrent Reserve calls both read the same `QuantityOnHand` under READ COMMITTED, both pass the availability check, and both insert reservations — total reserved qty exceeds on-hand (over-reserve). UPDLOCK turns the shared read lock into an exclusive-intent lock so the second reader blocks until the first transaction commits, then reads the already-updated balance.

---

### 5. `InventoryBalanceAccess.cs`

```
Backend/src/Modules/Inventory/PrismERP.Modules.Inventory.Application/Inventory/Internal/InventoryBalanceAccess.cs
```

New method **`GetForUpdateWithLockByProductWarehouseAsync`** — same two-step lookup as `GetForUpdateByProductWarehouseAsync` but delegates to `GetByIdForUpdateWithLockAsync` for the UPDLOCK step.

---

### 6. `InventorySalesReservationWorkflowService.cs`

```
Backend/src/Modules/Inventory/PrismERP.Modules.Inventory.Application/Inventory/Workflows/InventorySalesReservationWorkflowService.cs
```

In `ReserveForSalesOrderAsync`, the balance load was changed from `GetForUpdateByProductWarehouseAsync` → **`GetForUpdateWithLockByProductWarehouseAsync`**.

This is the single call site for all SO Approve reservations. No other change to the existing Reserve logic was needed.

---

### 7. `SalesOrderService.ApproveAsync`

```
Backend/src/Modules/SalesOrdering/PrismERP.Modules.SalesOrdering.Application/SalesOrders/SalesOrderService.cs
```

Before (pseudocode):
```
load SO outside TX (stale)
begin TX
  ReserveForSalesOrderAsync(...)   // with stale SO lines
  SO.Approve()
  SaveChanges
commit
```

After:
```
ExecuteInTransactionWithRetryAsync:
  db.ChangeTracker.Clear()            // fresh state per attempt
  begin TX
    reload SO ForUpdate               // always fresh; ChangeTracker cleared above
    if SO.Status != Draft → throw ConflictException (409, no retry)
    ReserveForSalesOrderAsync(...)    // UPDLOCK on balance row
    SO.Approve()
    SaveChanges                       // RowVersion on SO; DbConcurrency → retry
  commit
```

**Scenarios handled:**

| Scenario | Outcome |
|---|---|
| User A and User B approve **the same SO** | First commit wins. Second reload sees `Status=Confirmed` → `ConflictException` (409). |
| User A and User B approve **different SOs** for the same product | UPDLOCK serialises balance reads; second tx waits and reads correct available qty. No over-reserve. |
| Any RowVersion conflict on SO row | Retry → reload → status check → proceed or 409. |

---

### 8. `DeliveryNoteService.PostAsync`

```
Backend/src/Modules/SalesOrdering/PrismERP.Modules.SalesOrdering.Application/Deliveries/DeliveryNoteService.cs
```

Before (pseudocode):
```
load DN + SO outside TX (stale)
load reservations outside TX (stale)
build fulfillLines with stale reservation objects
begin TX
  FulfillReservationsAsync(stale fulfillLines)
  DN.Post(SO)
  Create Invoice
  SaveChanges
commit
```

After:
```
ExecuteInTransactionWithRetryAsync:
  db.ChangeTracker.Clear()
  begin TX
    reload DN ForUpdate              // fresh
    if DN.Status != Draft → throw ConflictException (409, no retry)
    reload SO ForUpdate              // fresh
    load reservations INSIDE TX      // reads latest committed quantities, not stale snapshot
    build fulfillLines with fresh reservation objects
    FulfillReservationsAsync(...)    // RowVersion on Balance → DbConcurrency → retry
    DN.Post(SO)
    Create Invoice
    SaveChanges
  commit
```

**Key fix: reservations loaded inside the transaction.**  
Previously, `GetActivesByReferencesAsync` was called before `ExecuteInTransactionAsync`. If a concurrent DN post had already partially fulfilled a reservation, the outer load would still see the old `RemainingQuantity`, pass the qty check, and then `SaveChanges` would fail or silently over-fulfill. Moving the load inside the transaction guarantees that the `RemainingQuantity` checked is the one that will be written.

**Scenarios handled:**

| Scenario | Outcome |
|---|---|
| Two users post the **same DN** | First commit wins. Second reload sees `Status=Posted` → `ConflictException` (409). |
| Two **different DNs** for same SO line (partial deliveries) | RowVersion on reservation/balance causes `DbUpdateConcurrencyException` → retry → fresh reload → correct remaining qty → proceed or 422. |
| Client F5s mid-post | `OperationCanceledException` → logged at Info, no 500 error. |

---

## Concurrency decision tree (summary)

```
Exception during Approve / Post
├── ConflictException (409)
│   ├── Same SO approved twice → "Sales order is already Confirmed"
│   └── Same DN posted twice  → "Delivery note is already Posted"
├── BusinessException (422)
│   └── Qty exceeds remaining reservation after fresh reload
├── DbUpdateConcurrencyException
│   ├── attempt < 3 → ChangeTracker.Clear() + retry
│   └── attempt = 3 → ConflictException (409) "Data was modified. Please refresh."
└── OperationCanceledException
    └── client disconnected → LogInformation, no response
```

---

## What was NOT changed

- `CancelAsync` on both `SalesOrderService` and `DeliveryNoteService` — cancellation is a simpler path with lower concurrency risk and is left using the existing `ExecuteInTransactionAsync`.
- `InventoryAvailabilityChecker.EnsureAvailableAsync` — not modified; the UPDLOCK acquired before calling it is sufficient to serialise the read-check-insert sequence.
- No migration was added — UPDLOCK is a query-time hint, not a schema change.
- No changes to the FE — 409 responses are handled by `parseApiError` / `getToastMessage` which already surfaces the `title` field from `ProblemDetails`.
