# PrismERP — CV Description

---

## ENGLISH VERSION

### Short (1–2 lines, for skills section or project list)

**PrismERP** | .NET 9, React, SQL Server, Redis | [github.com/your-username/PrismERP](https://github.com)  
Modular monolith ERP with Purchasing, Sales, and Inventory modules; FIFO costing, inventory reservations, policy-based authorization, and 18 SQL Server integration tests including concurrency scenarios.

---

### Medium (3–5 lines, for project section in CV)

**PrismERP — Modular Monolith ERP** | Personal Project | 2025–2026  
*Stack: .NET 9 · ASP.NET Core · Entity Framework Core · SQL Server · React · Redux Toolkit · Redis*

- Designed and implemented a modular monolith with 5 bounded contexts (Identity, Inventory, Purchasing, Sales, Finance), each with its own DB schema, UnitOfWork, and service layer.
- Built end-to-end business flows: PO approval → Goods Receipt → FIFO inventory receive; SO approval → Delivery Note → inventory reservation & FIFO issue; automatic invoice generation.
- Implemented optimistic concurrency with UPDLOCK, ordered locking to prevent deadlocks, and transaction retry — verified with 18 integration tests running against a real SQL Server instance.
- Designed permission-based authorization (RBAC): JWT carries roles; a per-request middleware loads permission codes from DB; custom `IAuthorizationHandler` enforces policies on every API action.
- Frontend: React SPA with Redux Toolkit state management, JWT in memory + HttpOnly refresh-token cookie, and a clean cancel/approve/post workflow across all modules.

---

### Long (full paragraph for portfolio page or LinkedIn)

**PrismERP** is a full-stack ERP portfolio project built to demonstrate backend engineering skills beyond basic CRUD. The backend is a .NET 9 modular monolith with five modules — Identity, Inventory, Purchasing, Sales Ordering, and Finance — each isolated by its own SQL Server schema, repositories, and UnitOfWork, while sharing a single EF Core DbContext to enable cross-module transactions without distributed transaction complexity.

Key technical highlights:

**Domain-Driven Design:** Domain entities (SalesOrder, PurchaseOrder, InventoryBalance) own their invariants. Business actions like `Approve()` and `Cancel()` validate state transitions within the entity; services only orchestrate: load → validate → call domain method → save.

**Inventory mechanics:** FIFO cost-layer model. On GR Post, an `InventoryCostLayer` is created and `QuantityOnHand` updated. On DN Post, a FIFO issuer consumes layers oldest-first and records `InventoryMovement` per layer. Reservations track available stock for approved sales orders.

**Concurrency:** UPDLOCK on `InventoryBalance` rows serializes concurrent reservations. Locks are acquired in ascending ID order to prevent deadlocks. A transaction-with-retry loop (max 3 retries, `ChangeTracker.Clear()` between attempts) handles `DbUpdateConcurrencyException`. All concurrency scenarios are covered by integration tests.

**Authorization:** RBAC with runtime permission hydration — JWT holds only roles; a middleware loads permission codes per request; `[RequirePermission("salesorder:approve")]` attribute enforces fine-grained policies on each controller action. Super-admin bypasses via a `*` shortcut.

**Testing:** 18 xUnit integration tests on real SQL Server, covering happy paths, business-rule violations, and concurrent approve/post scenarios using a `Barrier`-based gate pattern.

**Frontend:** React + Vite SPA, Redux Toolkit for state, axios with request interceptor for Bearer injection and 401 redirect, HttpOnly cookie for refresh token, confirm dialogs and role-aware button visibility.

---

## PHIÊN BẢN TIẾNG VIỆT

### Ngắn (1–2 dòng, mục kỹ năng hoặc danh sách project)

**PrismERP** | .NET 9, React, SQL Server, Redis | [github.com/your-username/PrismERP](https://github.com)  
Modular monolith ERP gồm Mua hàng, Bán hàng, Kho; tính giá FIFO, đặt chỗ tồn kho, phân quyền theo permission, 18 integration test trên SQL Server thật bao gồm kiểm thử xử lý đồng thời.

---

### Trung bình (3–5 dòng, mục dự án trong CV)

**PrismERP — Modular Monolith ERP** | Dự án cá nhân | 2025–2026  
*Stack: .NET 9 · ASP.NET Core · Entity Framework Core · SQL Server · React · Redux Toolkit · Redis*

- Thiết kế và xây dựng modular monolith với 5 bounded context (Identity, Inventory, Purchasing, Sales, Finance), mỗi module có schema DB riêng, UnitOfWork riêng, và service layer riêng.
- Xây dựng luồng nghiệp vụ end-to-end: Duyệt PO → Nhận hàng (GR) → Tính giá FIFO; Duyệt SO → Xuất hàng (DN) → Đặt chỗ tồn + FIFO issue; tự động tạo invoice.
- Xử lý đồng thời với UPDLOCK, khóa theo thứ tự để tránh deadlock, retry transaction — kiểm chứng bằng 18 integration tests chạy trên SQL Server thật.
- Thiết kế phân quyền theo permission (RBAC): JWT mang roles; middleware load permission codes từ DB mỗi request; custom `IAuthorizationHandler` kiểm tra policy trên từng API action.
- Frontend: React SPA với Redux Toolkit, JWT lưu trong memory + HttpOnly refresh-token cookie, luồng cancel/approve/post đầy đủ cho tất cả module.

---

### Dài (đoạn mô tả đầy đủ cho portfolio hoặc LinkedIn)

**PrismERP** là dự án ERP full-stack được xây dựng nhằm thể hiện kỹ năng backend vượt ra ngoài CRUD cơ bản. Backend là một modular monolith .NET 9 với năm module — Identity, Inventory, Purchasing, Sales Ordering và Finance — mỗi module được cô lập bằng schema SQL Server riêng, repository và UnitOfWork riêng, đồng thời chia sẻ một EF Core DbContext duy nhất để hỗ trợ giao dịch cross-module mà không cần distributed transaction.

**Domain-Driven Design:** Domain entity (SalesOrder, PurchaseOrder, InventoryBalance) tự bảo vệ business rule. Các thao tác như `Approve()` và `Cancel()` kiểm tra điều kiện trạng thái ngay trong entity; service chỉ orchestrate: load → validate → gọi method domain → save.

**Cơ chế kho hàng:** Mô hình FIFO cost layer. Khi Post GR, tạo `InventoryCostLayer` và cập nhật `QuantityOnHand`. Khi Post DN, FIFO issuer tiêu thụ layer theo thứ tự cũ nhất trước và ghi `InventoryMovement` theo từng layer. Reservation theo dõi tồn khả dụng cho SO đã được duyệt.

**Xử lý đồng thời:** UPDLOCK trên `InventoryBalance` serialize các reservation đồng thời. Lock được acquire theo thứ tự ID tăng dần để tránh deadlock. Vòng lặp retry transaction (tối đa 3 lần, `ChangeTracker.Clear()` giữa các lần) xử lý `DbUpdateConcurrencyException`. Tất cả kịch bản concurrency đều có integration test.

**Phân quyền:** RBAC với permission hydration tại runtime — JWT chỉ mang roles; middleware load permission codes mỗi request; attribute `[RequirePermission("salesorder:approve")]` enforce policy chi tiết trên từng action controller. Super-admin bypass qua shortcut `*`.

**Kiểm thử:** 18 xUnit integration tests chạy trên SQL Server thật, bao gồm happy path, vi phạm business rule và kịch bản approve/post đồng thời sử dụng `Barrier`-based gate pattern.

**Frontend:** React + Vite SPA, Redux Toolkit quản lý state, axios với request interceptor inject Bearer token và redirect khi 401, HttpOnly cookie cho refresh token, confirm dialog và ẩn/hiện nút theo role.

---

## Ghi chú sử dụng

- **Thay link GitHub** trước khi gửi CV
- Bản **ngắn** dùng cho CV 1 trang hoặc ô Skills
- Bản **trung bình** dùng cho mục Projects trong CV chi tiết
- Bản **dài** dùng cho portfolio website, LinkedIn featured section, hoặc email giới thiệu
- Kết hợp với `INTERVIEW.md` để chuẩn bị trả lời câu hỏi kỹ thuật chi tiết
