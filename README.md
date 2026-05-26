# Prism Cuisine

Modular monolith backend (.NET 9) and React frontend.

## Structure

- `Backend/` — ASP.NET Core API, Clean Architecture, CQRS, RabbitMQ, Redis, PostgreSQL (schema per module)
- `Frontend/prism-cuisine-web/` — React + Vite + Redux Toolkit

## Modules (database schemas)

| Module     | Schema        |
|-----------|---------------|
| Identity  | `identity`    |
| Inventory | `inventory`   |
| Purchasing| `purchasing`  |
| SalesOrder| `sales_order` |

## Run with Docker

```bash
docker compose up --build
```

- API: http://localhost:8080
- Web: http://localhost:3000
- RabbitMQ management: http://localhost:15672 (guest/guest)

## Local development

### Backend

```bash
cd Backend
dotnet run --project src/Api/PrismCuisine.Api
```

Requires PostgreSQL, Redis, and RabbitMQ (or use `docker compose up postgres redis rabbitmq`).

### Frontend

```bash
cd Frontend/prism-cuisine-web
cp .env.example .env
npm install
npm run dev
```
