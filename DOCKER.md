# Docker — PrismERP

SQL Server **không** chạy trong Docker — dùng instance trên máy (SSMS). Compose chỉ chạy Redis + API + Web.

## Yêu cầu

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- SQL Server đã cài trên máy, bật TCP, có login **sa** / **admin1** (hoặc sửa `.env`)
- Database `PrismERP` (API tự migrate lần đầu chạy nếu đã cấu hình EF migrate)

## Khởi động

```powershell
cd d:\Personals\PrismERP
docker compose up --build
```

| Service | URL |
|---------|-----|
| Web (React) | http://localhost:5173 |
| API | http://localhost:5085 |
| Scalar | http://localhost:5085/scalar/v1 |
| SignalR | ws://localhost:5085/hubs/notifications |
| Redis (Docker) | localhost:6379 |
| SQL Server | **Máy host** — mở SSMS: `localhost` hoặc `localhost\TÊN_INSTANCE` |

## Kết nối SQL

| Cách chạy API | Connection string server |
|---------------|--------------------------|
| API trong Docker | `host.docker.internal` (default trong compose) |
| API local (`dotnet run` / F5) | `localhost` — xem `appsettings.Development.json` |
| SSMS trên máy bạn | `localhost` hoặc `(local)\SQLEXPRESS` tùy instance |

Đổi server/password trong `.env`:

```env
SQL_CONNECTION_STRING=Server=host.docker.internal,1433;Database=PrismERP;User Id=sa;Password=admin1;TrustServerCertificate=True;
```

**Named instance** (vd. SQLEXPRESS):

```env
SQL_CONNECTION_STRING=Server=host.docker.internal\SQLEXPRESS;Database=PrismERP;User Id=sa;Password=admin1;TrustServerCertificate=True;
```

## Dev local (không Docker cho API/FE)

```powershell
docker compose up redis -d
```

- SQL: SSMS / SQL Server trên máy
- API: F5 hoặc `dotnet run` → http://localhost:5085
- FE: `npm run dev` → http://localhost:5173

## Dừng

```powershell
docker compose down
```

## Ghi chú

- Lỗi SQL container exit 255 thường do RAM/EULA/password — không còn áp dụng vì đã bỏ SQL khỏi compose.
- Nếu API container không kết nối được SQL host: bật **TCP/IP** trong SQL Server Configuration Manager, firewall cho port 1433, và SQL Authentication (Mixed Mode).
