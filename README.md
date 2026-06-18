# Prism ERP

Modular monolith backend (.NET 9) and React frontend.

## Structure

- `Backend/` — ASP.NET Core API, Clean Architecture, Unit of Work + Repository + Service, SignalR, Redis, SQL Server (schema per module)
- `Frontend/prism-erp-web/` — React + Vite + Redux Toolkit

## Modules (database schemas)

| Module     | Schema        |
|-----------|---------------|
| Identity  | `identity`    |
| Inventory | `inventory`   |
| Purchasing| `purchasing`  |
| SalesOrdering| `sales_order` |

## Application flow

```
Controller → Service (use case) → UnitOfWork / Repository → DbContext
  Read:  repository with AsNoTracking()
  Write: repository with tracking + domain behavior + SaveChanges()
```

## Run with Docker

See [DOCKER.md](./DOCKER.md).

```powershell
cd d:\Personals\PrismERP
docker compose up --build
```

- Web: http://localhost:5173
- API: http://localhost:5085
- SQL Server: trên **máy host** (SSMS), không trong Docker — mặc định sa / `admin1`

## Local development

### Backend

```bash
cd Backend
dotnet run --project src/Api/PrismERP.Api
```

Requires SQL Server, Redis, and RabbitMQ (or use `docker compose up sqlserver redis rabbitmq`).

### EF migrations

```powershell
cd Backend
dotnet ef migrations add <Name> `
  -Project src/BuildingBlocks/PrismERP.BuildingBlocks.Infrastructure `
  -StartupProject src/Api/PrismERP.Api `
  -OutputDir Persistence/Migrations

dotnet ef database update `
  -Project src/BuildingBlocks/PrismERP.BuildingBlocks.Infrastructure `
  -StartupProject src/Api/PrismERP.Api
```

### Frontend

```bash
cd Frontend/prism-erp-web
cp .env.example .env
npm install
npm run dev
```
