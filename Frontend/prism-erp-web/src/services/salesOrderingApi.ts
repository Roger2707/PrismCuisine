import apiClient from './apiClient';
import type {
  CustomerDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  SalesOrderSummaryDto,
  SalesOrderDto,
  CreateSalesOrderRequest,
  UpdateSalesOrderRequest,
  DeliveryNoteSummaryDto,
  DeliveryNoteDto,
  CreateDeliveryNoteRequest,
  UpdateDeliveryNoteRequest,
} from './types/salesOrdering.types';

// Sales Ordering/Customers Module
export const customersApi = {
  getAll: async (): Promise<CustomerDto[]> => {
    const response = await apiClient.get<CustomerDto[]>('/api/sales-ordering/customers');
    return response.data;
  },
  getById: async (id: number): Promise<CustomerDto> => {
    const response = await apiClient.get<CustomerDto>(`/api/sales-ordering/customers/${id}`);
    return response.data;
  },
  create: async (request: CreateCustomerRequest): Promise<CustomerDto> => {
    const response = await apiClient.post<CustomerDto>('/api/sales-ordering/customers', request);
    return response.data;
  },
  update: async (id: number, request: UpdateCustomerRequest): Promise<void> => {
    await apiClient.put(`/api/sales-ordering/customers/${id}`, request);
  },
  deactivate: async (id: number): Promise<void> => {
    await apiClient.post(`/api/sales-ordering/customers/${id}/deactivate`);
  },
};

// Sales Ordering/Sales Orders Module
export const salesOrdersApi = {
  getAll: async (): Promise<SalesOrderSummaryDto[]> => {
    const response = await apiClient.get<SalesOrderSummaryDto[]>('/api/sales-ordering/sales-orders');
    return response.data;
  },
  getById: async (id: number): Promise<SalesOrderDto> => {
    const response = await apiClient.get<SalesOrderDto>(`/api/sales-ordering/sales-orders/${id}`);
    return response.data;
  },
  create: async (request: CreateSalesOrderRequest): Promise<SalesOrderDto> => {
    const response = await apiClient.post<SalesOrderDto>('/api/sales-ordering/sales-orders', request);
    return response.data;
  },
  update: async (id: number, request: UpdateSalesOrderRequest): Promise<void> => {
    await apiClient.put(`/api/sales-ordering/sales-orders/${id}`, request);
  },
  approve: async (id: number): Promise<void> => {
    await apiClient.post(`/api/sales-ordering/sales-orders/${id}/approve`);
  },
  cancel: async (id: number): Promise<void> => {
    await apiClient.post(`/api/sales-ordering/sales-orders/${id}/cancel`);
  },
};

// Sales Ordering/Delivery Notes Module
export const deliveryNotesApi = {
  getAll: async (): Promise<DeliveryNoteSummaryDto[]> => {
    const response = await apiClient.get<DeliveryNoteSummaryDto[]>('/api/sales-ordering/delivery-notes');
    return response.data;
  },
  getById: async (id: number): Promise<DeliveryNoteDto> => {
    const response = await apiClient.get<DeliveryNoteDto>(`/api/sales-ordering/delivery-notes/${id}`);
    return response.data;
  },
  create: async (request: CreateDeliveryNoteRequest): Promise<DeliveryNoteDto> => {
    const response = await apiClient.post<DeliveryNoteDto>('/api/sales-ordering/delivery-notes', request);
    return response.data;
  },
  update: async (id: number, request: UpdateDeliveryNoteRequest): Promise<void> => {
    await apiClient.put(`/api/sales-ordering/delivery-notes/${id}`, request);
  },
  post: async (id: number): Promise<void> => {
    await apiClient.post(`/api/sales-ordering/delivery-notes/${id}/post`);
  },
  cancel: async (id: number): Promise<void> => {
    await apiClient.post(`/api/sales-ordering/delivery-notes/${id}/cancel`);
  },
};
