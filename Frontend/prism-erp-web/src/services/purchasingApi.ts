import apiClient from './apiClient';
import type {
  SupplierDto,
  CreateSupplierRequest,
  UpdateSupplierRequest,
  PurchaseOrderSummaryDto,
  PurchaseOrderDto,
  CreatePurchaseOrderRequest,
  AddPurchaseOrderLineRequest,
  UpdatePurchaseOrderRequest,
  CreatePurchaseOrderAmendmentRequest,
  GoodsReceiptSummaryDto,
  GoodsReceiptDto,
  CreateGoodsReceiptRequest,
  UpdateGoodsReceiptRequest,
} from './types/purchasing.types';

// Purchasing/Suppliers Module
export const suppliersApi = {
  getAll: async (): Promise<SupplierDto[]> => {
    const response = await apiClient.get<SupplierDto[]>('/api/purchasing/suppliers');
    return response.data;
  },
  getById: async (id: number): Promise<SupplierDto> => {
    const response = await apiClient.get<SupplierDto>(`/api/purchasing/suppliers/${id}`);
    return response.data;
  },
  create: async (request: CreateSupplierRequest): Promise<SupplierDto> => {
    const response = await apiClient.post<SupplierDto>('/api/purchasing/suppliers', request);
    return response.data;
  },
  update: async (id: number, request: UpdateSupplierRequest): Promise<void> => {
    await apiClient.put(`/api/purchasing/suppliers/${id}`, request);
  },
  deactivate: async (id: number): Promise<void> => {
    await apiClient.post(`/api/purchasing/suppliers/${id}/deactivate`);
  },
};

// Purchasing/Purchase Orders Module
export const purchaseOrdersApi = {
  getAll: async (): Promise<PurchaseOrderSummaryDto[]> => {
    const response = await apiClient.get<PurchaseOrderSummaryDto[]>('/api/purchasing/purchase-orders');
    return response.data;
  },
  getById: async (id: number): Promise<PurchaseOrderDto> => {
    const response = await apiClient.get<PurchaseOrderDto>(`/api/purchasing/purchase-orders/${id}`);
    return response.data;
  },
  create: async (request: CreatePurchaseOrderRequest): Promise<PurchaseOrderDto> => {
    const response = await apiClient.post<PurchaseOrderDto>('/api/purchasing/purchase-orders', request);
    return response.data;
  },
  update: async (id: number, request: UpdatePurchaseOrderRequest): Promise<void> => {
    await apiClient.put(`/api/purchasing/purchase-orders/${id}`, request);
  },
  createAmendment: async (id: number, request: CreatePurchaseOrderAmendmentRequest): Promise<PurchaseOrderDto> => {
    const response = await apiClient.post<PurchaseOrderDto>(`/api/purchasing/purchase-orders/${id}/amendment`, request);
    return response.data;
  },
  addLine: async (id: number, request: AddPurchaseOrderLineRequest): Promise<void> => {
    await apiClient.post(`/api/purchasing/purchase-orders/${id}/lines`, request);
  },
  approve: async (id: number): Promise<void> => {
    await apiClient.post(`/api/purchasing/purchase-orders/${id}/approve`);
  },
  cancel: async (id: number): Promise<void> => {
    await apiClient.post(`/api/purchasing/purchase-orders/${id}/cancel`);
  },
};

// Purchasing/Goods Receipts Module
export const goodsReceiptsApi = {
  getByPurchaseOrder: async (purchaseOrderId: number): Promise<GoodsReceiptSummaryDto[]> => {
    const response = await apiClient.get<GoodsReceiptSummaryDto[]>(`/api/purchasing/goods-receipts/by-purchase-order/${purchaseOrderId}`);
    return response.data;
  },
  getById: async (id: number): Promise<GoodsReceiptDto> => {
    const response = await apiClient.get<GoodsReceiptDto>(`/api/purchasing/goods-receipts/${id}`);
    return response.data;
  },
  create: async (request: CreateGoodsReceiptRequest): Promise<GoodsReceiptDto> => {
    const response = await apiClient.post<GoodsReceiptDto>('/api/purchasing/goods-receipts', request);
    return response.data;
  },
  update: async (id: number, request: UpdateGoodsReceiptRequest): Promise<void> => {
    await apiClient.put(`/api/purchasing/goods-receipts/${id}`, request);
  },
  addLine: async (id: number, request: AddPurchaseOrderLineRequest): Promise<void> => {
    await apiClient.post(`/api/purchasing/goods-receipts/${id}/lines`, request);
  },
  post: async (id: number): Promise<GoodsReceiptDto> => {
    const response = await apiClient.post<GoodsReceiptDto>(`/api/purchasing/goods-receipts/${id}/post`);
    return response.data;
  },
  cancel: async (id: number): Promise<void> => {
    await apiClient.post(`/api/purchasing/goods-receipts/${id}/cancel`);
  },
};
