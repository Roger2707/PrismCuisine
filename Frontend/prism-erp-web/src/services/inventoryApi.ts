import apiClient from './apiClient';
import type {
  ProductDto,
  CreateProductRequest,
  UpdateProductRequest,
  ProductCategoryDto,
  CreateProductCategoryRequest,
  UpdateProductCategoryRequest,
  WarehouseDto,
  CreateWarehouseRequest,
  UpdateWarehouseRequest,
  InventoryBalanceDto,
  InventoryMovementDto,
  InventoryCostLayerDto,
  InventoryReservationDto,
  CreateInventoryBalanceRequest,
  ReceiveInventoryRequest,
  IssueInventoryRequest,
  AdjustInventoryRequest,
  CreateReservationRequest,
} from './types/inventory.types';

// Inventory/Products Module
export const productsApi = {
  getAll: async (): Promise<ProductDto[]> => {
    const response = await apiClient.get<ProductDto[]>('/api/inventory/products');
    return response.data;
  },
  getById: async (id: number): Promise<ProductDto> => {
    const response = await apiClient.get<ProductDto>(`/api/inventory/products/${id}`);
    return response.data;
  },
  getBySku: async (sku: string): Promise<ProductDto> => {
    const response = await apiClient.get<ProductDto>(`/api/inventory/products/by-sku/${sku}`);
    return response.data;
  },
  create: async (request: CreateProductRequest): Promise<ProductDto> => {
    const response = await apiClient.post<ProductDto>('/api/inventory/products', request);
    return response.data;
  },
  update: async (id: number, request: UpdateProductRequest): Promise<void> => {
    await apiClient.put(`/api/inventory/products/${id}`, request);
  },
  deactivate: async (id: number): Promise<void> => {
    await apiClient.post(`/api/inventory/products/${id}/deactivate`);
  },
};

// Inventory/Product Categories Module
export const productCategoriesApi = {
  getAll: async (): Promise<ProductCategoryDto[]> => {
    const response = await apiClient.get<ProductCategoryDto[]>('/api/inventory/product-categories');
    return response.data;
  },
  getById: async (id: number): Promise<ProductCategoryDto> => {
    const response = await apiClient.get<ProductCategoryDto>(`/api/inventory/product-categories/${id}`);
    return response.data;
  },
  create: async (request: CreateProductCategoryRequest): Promise<ProductCategoryDto> => {
    const response = await apiClient.post<ProductCategoryDto>('/api/inventory/product-categories', request);
    return response.data;
  },
  update: async (id: number, request: UpdateProductCategoryRequest): Promise<void> => {
    await apiClient.put(`/api/inventory/product-categories/${id}`, request);
  },
};

// Inventory/Warehouses Module
export const warehousesApi = {
  getAll: async (): Promise<WarehouseDto[]> => {
    const response = await apiClient.get<WarehouseDto[]>('/api/inventory/warehouses');
    return response.data;
  },
  getById: async (id: number): Promise<WarehouseDto> => {
    const response = await apiClient.get<WarehouseDto>(`/api/inventory/warehouses/${id}`);
    return response.data;
  },
  create: async (request: CreateWarehouseRequest): Promise<WarehouseDto> => {
    const response = await apiClient.post<WarehouseDto>('/api/inventory/warehouses', request);
    return response.data;
  },
  update: async (id: number, request: UpdateWarehouseRequest): Promise<void> => {
    await apiClient.put(`/api/inventory/warehouses/${id}`, request);
  },
};

// Inventory/Balances Module
export const inventoryApi = {
  getLowStock: async (): Promise<InventoryBalanceDto[]> => {
    const response = await apiClient.get<InventoryBalanceDto[]>('/api/inventory/balances/low-stock');
    return response.data;
  },
  getBalanceById: async (id: number): Promise<InventoryBalanceDto> => {
    const response = await apiClient.get<InventoryBalanceDto>(`/api/inventory/balances/${id}`);
    return response.data;
  },
  getBalance: async (productId: number, warehouseId: number): Promise<InventoryBalanceDto> => {
    const response = await apiClient.get<InventoryBalanceDto>('/api/inventory/balances', {
      params: { productId, warehouseId },
    });
    return response.data;
  },
  ensureBalance: async (request: CreateInventoryBalanceRequest): Promise<InventoryBalanceDto> => {
    const response = await apiClient.post<InventoryBalanceDto>('/api/inventory/balances', request);
    return response.data;
  },
  getMovements: async (id: number): Promise<InventoryMovementDto[]> => {
    const response = await apiClient.get<InventoryMovementDto[]>(`/api/inventory/balances/${id}/movements`);
    return response.data;
  },
  getCostLayers: async (id: number): Promise<InventoryCostLayerDto[]> => {
    const response = await apiClient.get<InventoryCostLayerDto[]>(`/api/inventory/balances/${id}/cost-layers`);
    return response.data;
  },
  receive: async (request: ReceiveInventoryRequest): Promise<InventoryMovementDto> => {
    const response = await apiClient.post<InventoryMovementDto>('/api/inventory/receive', request);
    return response.data;
  },
  issue: async (request: IssueInventoryRequest): Promise<InventoryMovementDto> => {
    const response = await apiClient.post<InventoryMovementDto>('/api/inventory/issue', request);
    return response.data;
  },
  adjust: async (request: AdjustInventoryRequest): Promise<InventoryMovementDto> => {
    const response = await apiClient.post<InventoryMovementDto>('/api/inventory/adjust', request);
    return response.data;
  },
  getReservation: async (id: number): Promise<InventoryReservationDto> => {
    const response = await apiClient.get<InventoryReservationDto>(`/api/inventory/reservations/${id}`);
    return response.data;
  },
  reserve: async (request: CreateReservationRequest): Promise<InventoryReservationDto[]> => {
    const response = await apiClient.post<InventoryReservationDto[]>('/api/inventory/reservations', request);
    return response.data;
  },
  releaseReservation: async (id: number): Promise<void> => {
    await apiClient.post(`/api/inventory/reservations/${id}/release`);
  },
};
