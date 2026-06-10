// Sales Ordering Module Types

export interface CustomerDto {
  id: number;
  code: string;
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
  isActive: boolean;
}

export interface CreateCustomerRequest {
  code: string;
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
}

export interface UpdateCustomerRequest {
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
}

export interface SalesOrderSummaryDto {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  orderDate: string;
  deliveryDate?: string;
  approvedAt?: string;
  status: string;
  notes?: string;
  subTotal: number;
  totalDiscount: number;
  totalVAT: number;
  totalAmount: number;
}

export interface SalesOrderLineDto {
  id: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityDelivered: number;
  quantityRemaining: number;
  unitPrice: number;
  discountPercent: number;
  vatRate: number;
  discountAmount: number;
  vatAmount: number;
  lineTotal: number;
}

export interface SalesOrderDto {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  orderDate: string;
  deliveryDate?: string;
  approvedAt?: string;
  status: string;
  notes?: string;
  subTotal: number;
  totalDiscount: number;
  totalVAT: number;
  totalAmount: number;
  lines: SalesOrderLineDto[];
}

export interface CreateSalesOrderLineRequest {
  productId: number;
  productName: string;
  quantityOrdered: number;
  unitPrice: number;
  discountPercent: number;
  vatRate: number;
}

export interface CreateSalesOrderRequest {
  customerId: number;
  customerName?: string;
  notes?: string;
  lines: CreateSalesOrderLineRequest[];
}

export interface UpdateSalesOrderRequest {
  customerId: number;
  customerName?: string;
  notes?: string;
  lines: CreateSalesOrderLineRequest[];
}

export interface DeliveryNoteSummaryDto {
  id: number;
  deliveryNumber: string;
  salesOrderId: number;
  customerId: number;
  customerName: string;
  orderNumber: string;
  deliveryDate: string;
  status: string;
  notes?: string;
}

export interface DeliveryNoteLineDto {
  id: number;
  salesOrderLineId: number;
  productId: number;
  productName: string;
  quantityDelivered: number;
}

export interface DeliveryNoteDto {
  id: number;
  deliveryNumber: string;
  salesOrderId: number;
  customerId: number;
  customerName: string;
  orderNumber: string;
  deliveryDate: string;
  status: string;
  notes?: string;
  lines: DeliveryNoteLineDto[];
}

export interface CreateDeliveryNoteLineRequest {
  salesOrderLineId: number;
  quantityDelivered: number;
}

export interface CreateDeliveryNoteRequest {
  salesOrderId: number;
  notes?: string;
  lines: CreateDeliveryNoteLineRequest[];
}

export interface UpdateDeliveryNoteRequest {
  notes?: string;
  lines: CreateDeliveryNoteLineRequest[];
}
