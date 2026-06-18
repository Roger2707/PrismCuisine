import apiClient from './apiClient';
import type {
  InvoiceDto,
  PaymentDto,
  CreatePaymentRequest,
  UpdatePaymentRequest,
  CreatePurchaseInvoiceRequest,
} from './types/finance.types';

async function getOrNull<T>(url: string): Promise<T | null> {
  try {
    const response = await apiClient.get<T>(url);
    return response.data;
  } catch (error: unknown) {
    const status = (error as { response?: { status?: number } })?.response?.status;
    if (status === 404) return null;
    throw error;
  }
}

export const invoicesApi = {
  getAll: async (): Promise<InvoiceDto[]> => {
    const response = await apiClient.get<InvoiceDto[]>('/api/finance/invoices');
    return response.data;
  },

  getById: async (id: number): Promise<InvoiceDto> => {
    const response = await apiClient.get<InvoiceDto>(`/api/finance/invoices/${id}`);
    return response.data;
  },

  getByDeliveryNote: (deliveryNoteId: number) =>
    getOrNull<InvoiceDto>(`/api/finance/invoices/by-delivery-note/${deliveryNoteId}`),

  getByGoodsReceipt: (goodsReceiptId: number) =>
    getOrNull<InvoiceDto>(`/api/finance/invoices/by-goods-receipt/${goodsReceiptId}`),
};

export const paymentsApi = {
  getAll: async (): Promise<PaymentDto[]> => {
    const response = await apiClient.get<PaymentDto[]>('/api/finance/payments');
    return response.data;
  },

  getById: async (id: number): Promise<PaymentDto> => {
    const response = await apiClient.get<PaymentDto>(`/api/finance/payments/${id}`);
    return response.data;
  },

  getByInvoice: async (invoiceId: number): Promise<PaymentDto[]> => {
    const response = await apiClient.get<PaymentDto[]>(`/api/finance/payments/by-invoice/${invoiceId}`);
    return response.data;
  },

  generateNumber: async (): Promise<string> => {
    const response = await apiClient.get<{ paymentNumber: string }>('/api/finance/payments/generate-number');
    return response.data.paymentNumber;
  },

  create: async (request: CreatePaymentRequest): Promise<PaymentDto> => {
    const response = await apiClient.post<PaymentDto>('/api/finance/payments', request);
    return response.data;
  },

  update: async (id: number, request: UpdatePaymentRequest): Promise<void> => {
    await apiClient.put(`/api/finance/payments/${id}`, request);
  },

  complete: async (id: number): Promise<void> => {
    await apiClient.post(`/api/finance/payments/${id}/complete`);
  },

  fail: async (id: number): Promise<void> => {
    await apiClient.post(`/api/finance/payments/${id}/fail`);
  },

  cancel: async (id: number): Promise<void> => {
    await apiClient.post(`/api/finance/payments/${id}/cancel`);
  },
};

export const purchaseInvoicesApi = {
  getAll: async (): Promise<InvoiceDto[]> => {
    const response = await apiClient.get<InvoiceDto[]>('/api/purchasing/purchase-invoices');
    return response.data;
  },

  getById: async (id: number): Promise<InvoiceDto> => {
    const response = await apiClient.get<InvoiceDto>(`/api/purchasing/purchase-invoices/${id}`);
    return response.data;
  },

  getByGoodsReceipt: (goodsReceiptId: number) =>
    getOrNull<InvoiceDto>(`/api/purchasing/purchase-invoices/by-goods-receipt/${goodsReceiptId}`),

  createFromGoodsReceipt: async (request: CreatePurchaseInvoiceRequest): Promise<InvoiceDto> => {
    const response = await apiClient.post<InvoiceDto>('/api/purchasing/purchase-invoices', request);
    return response.data;
  },
};
