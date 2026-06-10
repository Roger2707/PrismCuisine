const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5085';

export const api = {
  // Identity Module
  getUsers: async () => {
    const response = await fetch(`${API_BASE_URL}/api/users`);
    return response.json();
  },

  // Inventory Module
  getProducts: async () => {
    const response = await fetch(`${API_BASE_URL}/api/products`);
    return response.json();
  },

  // Purchasing Module
  getPurchaseOrders: async () => {
    const response = await fetch(`${API_BASE_URL}/api/purchasing/purchase-orders`);
    return response.json();

  },

  // Sales Ordering Module
  getSalesOrders: async () => {
    const response = await fetch(`${API_BASE_URL}/api/sales-orders`);
    return response.json();
  },

  createSalesOrder: async (order: any) => {
    const response = await fetch(`${API_BASE_URL}/api/sales-orders`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(order),
    });
    return response.json();
  },

  updateSalesOrder: async (id: number, order: any) => {
    const response = await fetch(`${API_BASE_URL}/api/sales-orders/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(order),
    });
    return response.json();
  },

  deleteSalesOrder: async (id: number) => {
    const response = await fetch(`${API_BASE_URL}/api/sales-orders/${id}`, {
      method: 'DELETE',
    });
    return response.json();
  },

  // Goods Receipt Module
  getGoodsReceipts: async () => {
    const response = await fetch(`${API_BASE_URL}/api/goods-receipts`);
    return response.json();
  },
};
