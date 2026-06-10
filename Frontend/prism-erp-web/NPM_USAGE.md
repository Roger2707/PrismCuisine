# Prism ERP Frontend - NPM Usage Guide

## Install dependencies

```bash
cd Frontend/prism-erp-web
npm install
```

## Required dependencies

To run the application, you need to install the following packages:

```bash
npm install axios
```

- **axios**: HTTP client for API calls (required for the new API service)

Optional dependencies:
```bash
npm install react-router-dom lucide-react
```

- **react-router-dom**: Routing in the application
- **lucide-react**: Modern icon library

## Run application in development mode

```bash
npm run dev
```

Application will run at: `http://localhost:5173`

## Build for production

```bash
npm run build
```

Build files will be created in the `dist/` directory

## Preview build

```bash
npm run preview
```

## Check code quality

```bash
npm run lint
```

## Environment configuration

Create a `.env` file in the `Frontend/prism-erp-web` directory:

```env
VITE_API_BASE_URL=http://localhost:5085
```

## Project structure

```
src/
├── components/      # Reusable components
│   ├── Login.tsx   # Login page
│   └── Sidebar.tsx # Navigation sidebar
├── pages/          # Main pages
│   ├── Dashboard.tsx      # Home page
│   ├── Identity.tsx       # User management module
│   ├── Inventory.tsx      # Inventory management module
│   ├── Purchasing.tsx     # Purchase management module
│   ├── SalesOrdering.tsx  # Sales management module (with CRUD)
│   └── GoodsReceipt.tsx   # Goods receipt module
├── services/       # API services
│   └── api.ts      # Axios-based API client with all backend endpoints
├── app/            # Redux store
└── App.tsx         # Main component
```

## Features

- ✅ Login page with email/password
- ✅ Dashboard with statistics overview
- ✅ Sidebar navigation with modern icons
- ✅ Identity Module: User management
- ✅ Inventory Module: Product management with balance display
- ✅ Purchasing Module: Purchase order management
- ✅ Sales Ordering Module: Full CRUD for sales orders
- ✅ Goods Receipt Module: Goods receipt management
- ✅ Modern, clean, user-friendly UI
- ✅ Axios-based API service with interceptors
- ✅ Automatic token handling
- ✅ Error handling with fallback to mock data

## API Service Structure

The new API service (`src/services/api.ts`) includes:

- **authApi**: Login, logout, current user, change password, refresh
- **usersApi**: Get user by ID
- **productsApi**: CRUD operations for products
- **productCategoriesApi**: CRUD operations for product categories
- **warehousesApi**: CRUD operations for warehouses
- **inventoryApi**: Balance management, movements, reservations
- **suppliersApi**: CRUD operations for suppliers
- **purchaseOrdersApi**: CRUD operations for purchase orders
- **goodsReceiptsApi**: CRUD operations for goods receipts
- **customersApi**: CRUD operations for customers
- **salesOrdersApi**: CRUD operations for sales orders
- **deliveryNotesApi**: CRUD operations for delivery notes

All API endpoints match the backend controller routes exactly.

## Notes

- Application uses axios with interceptors for automatic token handling
- All API calls have fallback to mock data if the backend is unavailable
- Login currently uses the backend API endpoint `/api/identity/auth/login`
- To rename the project in package.json from "prism-cuisine-web" to "prism-erp-web", run:
  ```bash
  npm pkg set name=prism-erp-web
  ```
