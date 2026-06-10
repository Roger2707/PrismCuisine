import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5085';

// Create axios instance with base configuration
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - maybe redirect to login
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Identity/Auth Module
export const authApi = {
  login: async (email: string, password: string) => {
    const response = await apiClient.post('/api/identity/auth/login', { email, password });
    return response.data;
  },
  logout: async () => {
    const response = await apiClient.post('/api/identity/auth/logout');
    return response.data;
  },
  getCurrentUser: async () => {
    const response = await apiClient.get('/api/identity/auth/current-user');
    return response.data;
  },
  changePassword: async (oldPassword: string, newPassword: string) => {
    const response = await apiClient.post('/api/identity/auth/change-password', {
      oldPassword,
      newPassword,
    });
    return response.data;
  },
  refreshPage: async () => {
    const response = await apiClient.post('/api/identity/auth/refresh-page');
    return response.data;
  },
};

// Identity/Users Module
export const usersApi = {
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/identity/users/${id}`);
    return response.data;
  },
};

// Inventory/Products Module
export const productsApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/inventory/products');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/products/${id}`);
    return response.data;
  },
  getBySku: async (sku: string) => {
    const response = await apiClient.get(`/api/inventory/products/by-sku/${sku}`);
    return response.data;
  },
  create: async (product: any) => {
    const response = await apiClient.post('/api/inventory/products', product);
    return response.data;
  },
  update: async (id: number, product: any) => {
    const response = await apiClient.put(`/api/inventory/products/${id}`, product);
    return response.data;
  },
  deactivate: async (id: number) => {
    const response = await apiClient.post(`/api/inventory/products/${id}/deactivate`);
    return response.data;
  },
};

// Inventory/Product Categories Module
export const productCategoriesApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/inventory/product-categories');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/product-categories/${id}`);
    return response.data;
  },
  create: async (category: any) => {
    const response = await apiClient.post('/api/inventory/product-categories', category);
    return response.data;
  },
  update: async (id: number, category: any) => {
    const response = await apiClient.put(`/api/inventory/product-categories/${id}`, category);
    return response.data;
  },
};

// Inventory/Warehouses Module
export const warehousesApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/inventory/warehouses');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/warehouses/${id}`);
    return response.data;
  },
  create: async (warehouse: any) => {
    const response = await apiClient.post('/api/inventory/warehouses', warehouse);
    return response.data;
  },
  update: async (id: number, warehouse: any) => {
    const response = await apiClient.put(`/api/inventory/warehouses/${id}`, warehouse);
    return response.data;
  },
};

// Inventory/Balances Module
export const inventoryApi = {
  getLowStock: async () => {
    const response = await apiClient.get('/api/inventory/balances/low-stock');
    return response.data;
  },
  getBalanceById: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/balances/${id}`);
    return response.data;
  },
  getBalance: async (productId: number, warehouseId: number) => {
    const response = await apiClient.get('/api/inventory/balances', {
      params: { productId, warehouseId },
    });
    return response.data;
  },
  ensureBalance: async (request: any) => {
    const response = await apiClient.post('/api/inventory/balances', request);
    return response.data;
  },
  getMovements: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/balances/${id}/movements`);
    return response.data;
  },
  getCostLayers: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/balances/${id}/cost-layers`);
    return response.data;
  },
  receive: async (request: any) => {
    const response = await apiClient.post('/api/inventory/receive', request);
    return response.data;
  },
  issue: async (request: any) => {
    const response = await apiClient.post('/api/inventory/issue', request);
    return response.data;
  },
  adjust: async (request: any) => {
    const response = await apiClient.post('/api/inventory/adjust', request);
    return response.data;
  },
  getReservation: async (id: number) => {
    const response = await apiClient.get(`/api/inventory/reservations/${id}`);
    return response.data;
  },
  reserve: async (request: any) => {
    const response = await apiClient.post('/api/inventory/reservations', request);
    return response.data;
  },
  releaseReservation: async (id: number) => {
    const response = await apiClient.post(`/api/inventory/reservations/${id}/release`);
    return response.data;
  },
};

// Purchasing/Suppliers Module
export const suppliersApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/purchasing/suppliers');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/purchasing/suppliers/${id}`);
    return response.data;
  },
  create: async (supplier: any) => {
    const response = await apiClient.post('/api/purchasing/suppliers', supplier);
    return response.data;
  },
  update: async (id: number, supplier: any) => {
    const response = await apiClient.put(`/api/purchasing/suppliers/${id}`, supplier);
    return response.data;
  },
  deactivate: async (id: number) => {
    const response = await apiClient.post(`/api/purchasing/suppliers/${id}/deactivate`);
    return response.data;
  },
};

// Purchasing/Purchase Orders Module
export const purchaseOrdersApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/purchasing/purchase-orders');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/purchasing/purchase-orders/${id}`);
    return response.data;
  },
  create: async (order: any) => {
    const response = await apiClient.post('/api/purchasing/purchase-orders', order);
    return response.data;
  },
  update: async (id: number, order: any) => {
    const response = await apiClient.put(`/api/purchasing/purchase-orders/${id}`, order);
    return response.data;
  },
  createAmendment: async (id: number, amendment: any) => {
    const response = await apiClient.post(`/api/purchasing/purchase-orders/${id}/amendment`, amendment);
    return response.data;
  },
  addLine: async (id: number, line: any) => {
    const response = await apiClient.post(`/api/purchasing/purchase-orders/${id}/lines`, line);
    return response.data;
  },
  approve: async (id: number) => {
    const response = await apiClient.post(`/api/purchasing/purchase-orders/${id}/approve`);
    return response.data;
  },
  cancel: async (id: number) => {
    const response = await apiClient.post(`/api/purchasing/purchase-orders/${id}/cancel`);
    return response.data;
  },
};

// Purchasing/Goods Receipts Module
export const goodsReceiptsApi = {
  getByPurchaseOrder: async (purchaseOrderId: number) => {
    const response = await apiClient.get(`/api/purchasing/goods-receipts/by-purchase-order/${purchaseOrderId}`);
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/purchasing/goods-receipts/${id}`);
    return response.data;
  },
  create: async (receipt: any) => {
    const response = await apiClient.post('/api/purchasing/goods-receipts', receipt);
    return response.data;
  },
  update: async (id: number, receipt: any) => {
    const response = await apiClient.put(`/api/purchasing/goods-receipts/${id}`, receipt);
    return response.data;
  },
  addLine: async (id: number, line: any) => {
    const response = await apiClient.post(`/api/purchasing/goods-receipts/${id}/lines`, line);
    return response.data;
  },
  post: async (id: number) => {
    const response = await apiClient.post(`/api/purchasing/goods-receipts/${id}/post`);
    return response.data;
  },
};

// Sales Ordering/Customers Module
export const customersApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/sales-ordering/customers');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/sales-ordering/customers/${id}`);
    return response.data;
  },
  create: async (customer: any) => {
    const response = await apiClient.post('/api/sales-ordering/customers', customer);
    return response.data;
  },
  update: async (id: number, customer: any) => {
    const response = await apiClient.put(`/api/sales-ordering/customers/${id}`, customer);
    return response.data;
  },
  deactivate: async (id: number) => {
    const response = await apiClient.post(`/api/sales-ordering/customers/${id}/deactivate`);
    return response.data;
  },
};

// Sales Ordering/Sales Orders Module
export const salesOrdersApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/sales-ordering/sales-orders');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/sales-ordering/sales-orders/${id}`);
    return response.data;
  },
  create: async (order: any) => {
    const response = await apiClient.post('/api/sales-ordering/sales-orders', order);
    return response.data;
  },
  update: async (id: number, order: any) => {
    const response = await apiClient.put(`/api/sales-ordering/sales-orders/${id}`, order);
    return response.data;
  },
  approve: async (id: number) => {
    const response = await apiClient.post(`/api/sales-ordering/sales-orders/${id}/approve`);
    return response.data;
  },
  cancel: async (id: number) => {
    const response = await apiClient.post(`/api/sales-ordering/sales-orders/${id}/cancel`);
    return response.data;
  },
};

// Sales Ordering/Delivery Notes Module
export const deliveryNotesApi = {
  getAll: async () => {
    const response = await apiClient.get('/api/sales-ordering/delivery-notes');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await apiClient.get(`/api/sales-ordering/delivery-notes/${id}`);
    return response.data;
  },
  create: async (note: any) => {
    const response = await apiClient.post('/api/sales-ordering/delivery-notes', note);
    return response.data;
  },
  update: async (id: number, note: any) => {
    const response = await apiClient.put(`/api/sales-ordering/delivery-notes/${id}`, note);
    return response.data;
  },
  post: async (id: number) => {
    const response = await apiClient.post(`/api/sales-ordering/delivery-notes/${id}/post`);
    return response.data;
  },
  cancel: async (id: number) => {
    const response = await apiClient.post(`/api/sales-ordering/delivery-notes/${id}/cancel`);
    return response.data;
  },
};

// Export all APIs as a single object for backward compatibility
export const api = {
  ...authApi,
  ...usersApi,
  ...productsApi,
  ...productCategoriesApi,
  ...warehousesApi,
  ...inventoryApi,
  ...suppliersApi,
  ...purchaseOrdersApi,
  ...goodsReceiptsApi,
  ...customersApi,
  ...salesOrdersApi,
  ...deliveryNotesApi,
};

// Export the axios instance for custom requests
export default apiClient;
