# Prism ERP Frontend - Hướng dẫn sử dụng npm

## Cài đặt dependencies

```bash
cd Frontend/prism-erp-web
npm install
```

## Các dependencies cần thiết

Để chạy ứng dụng, bạn cần cài đặt các packages sau:

```bash
npm install react-router-dom lucide-react axios
```

- **react-router-dom**: Điều hướng trong ứng dụng
- **lucide-react**: Bộ icon hiện đại
- **axios**: Gọi API

## Chạy ứng dụng ở chế độ development

```bash
npm run dev
```

Ứng dụng sẽ chạy tại: `http://localhost:5173`

## Build cho production

```bash
npm run build
```

File build sẽ được tạo trong thư mục `dist/`

## Preview bản build

```bash
npm run preview
```

## Kiểm tra code quality

```bash
npm run lint
```

## Cấu hình biến môi trường

Tạo file `.env` trong thư mục `Frontend/prism-erp-web`:

```env
VITE_API_BASE_URL=http://localhost:5085
```

## Cấu trúc dự án

```
src/
├── components/      # Các component tái sử dụng
│   ├── Login.tsx   # Trang đăng nhập
│   └── Sidebar.tsx # Thanh điều hướng
├── pages/          # Các trang chính
│   ├── Dashboard.tsx      # Trang chủ
│   ├── Identity.tsx       # Module quản lý người dùng
│   ├── Inventory.tsx      # Module quản lý kho
│   ├── Purchasing.tsx     # Module quản lý mua hàng
│   └── SalesOrdering.tsx  # Module quản lý bán hàng (có CRUD)
├── app/            # Redux store
└── App.tsx         # Component chính
```

## Tính năng

- ✅ Trang đăng nhập với email/password
- ✅ Dashboard với thống kê tổng quan
- ✅ Sidebar điều hướng với icon hiện đại
- ✅ Module Identity: Hiển thị danh sách người dùng
- ✅ Module Inventory: Hiển thị danh sách sản phẩm
- ✅ Module Purchasing: Hiển thị danh sách đơn hàng mua
- ✅ Module SalesOrdering: CRUD đầy đủ cho đơn hàng bán
- ✅ UI hiện đại, sạch sẽ, dễ sử dụng

## Lưu ý

- Ứng dụng sử dụng mock data hiện tại. Để kết nối với API backend, cần cập nhật các API calls trong các component.
- Đăng nhập hiện tại chỉ là simulation. Cần tích hợp với API backend để xác thực thực sự.
- Để đổi tên project trong package.json từ "prism-cuisine-web" sang "prism-erp-web", chạy:
  ```bash
  npm pkg set name=prism-erp-web
  ```
