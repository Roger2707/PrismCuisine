# Prism Cuisine

Modular monolith backend (.NET 9) and React frontend.

## Structure

- `Backend/` — ASP.NET Core API, Clean Architecture, Unit of Work + Repository + Service, RabbitMQ, Redis, SQL Server (schema per module)
- `Frontend/prism-cuisine-web/` — React + Vite + Redux Toolkit

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

```bash
docker compose up --build
```

- API: http://localhost:8080
- Web: http://localhost:3000
- SQL Server: `localhost,1433` (sa / `Your_password123`)
- RabbitMQ management: http://localhost:15672 (guest/guest)

## Local development

### Backend

```bash
cd Backend
dotnet run --project src/Api/PrismCuisine.Api
```

Requires SQL Server, Redis, and RabbitMQ (or use `docker compose up sqlserver redis rabbitmq`).

### EF migrations

```powershell
cd Backend
dotnet ef migrations add <Name> `
  -Project src/BuildingBlocks/PrismCuisine.BuildingBlocks.Infrastructure `
  -StartupProject src/Api/PrismCuisine.Api `
  -OutputDir Persistence/Migrations

dotnet ef database update `
  -Project src/BuildingBlocks/PrismCuisine.BuildingBlocks.Infrastructure `
  -StartupProject src/Api/PrismCuisine.Api
```

### Frontend

```bash
cd Frontend/prism-cuisine-web
cp .env.example .env
npm install
npm run dev
```
