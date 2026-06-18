// Purchasing Module Types

export interface SupplierDto {
  id: number;
  code: string;
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
  isActive: boolean;
}

export interface CreateSupplierRequest {
  code: string;
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
}

export interface UpdateSupplierRequest {
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  taxCode?: string;
}

export interface PurchaseOrderSummaryDto {
  id: number;
  orderNumber: string;
  supplierId: number;
  warehouseId: number;
  status: string;
  invoiceStatus: string;
  amendedFromPurchaseOrderId?: number;
  approvedAt?: string;
  totalAmount: number;
}

export interface PurchaseOrderLineDto {
  id: number;
  productId: number;
  quantityOrdered: number;
  quantityReceived: number;
  quantityRemaining: number;
  unitPrice: number;
}

export interface PurchaseOrderDto {
  id: number;
  orderNumber: string;
  supplierId: number;
  warehouseId: number;
  status: string;
  invoiceStatus: string;
  amendedFromPurchaseOrderId?: number;
  approvedAt?: string;
  notes?: string;
  lines: PurchaseOrderLineDto[];
}

export interface CreatePurchaseOrderLineRequest {
  productId: number;
  quantityOrdered: number;
  unitPrice: number;
}

export interface CreatePurchaseOrderRequest {
  supplierId: number;
  warehouseId: number;
  notes?: string;
  lines: CreatePurchaseOrderLineRequest[];
}

export interface AddPurchaseOrderLineRequest {
  productId: number;
  quantityOrdered: number;
  unitPrice: number;
}

export interface UpdatePurchaseOrderRequest {
  supplierId: number;
  warehouseId: number;
  notes?: string;
  lines: CreatePurchaseOrderLineRequest[];
}

export interface CreatePurchaseOrderAmendmentRequest {
  notes?: string;
  copyRemainingLines?: boolean;
  lines?: CreatePurchaseOrderLineRequest[];
}

export interface GoodsReceiptSummaryDto {
  id: number;
  receiptNumber: string;
  purchaseOrderId: number;
  status: string;
  postedAt?: string;
}

export interface GoodsReceiptLineDto {
  id: number;
  purchaseOrderLineId: number;
  productId: number;
  quantity: number;
  unitCost: number;
}

export interface GoodsReceiptDto {
  id: number;
  receiptNumber: string;
  purchaseOrderId: number;
  status: string;
  postedAt?: string;
  notes?: string;
  lines: GoodsReceiptLineDto[];
}

export interface AddGoodsReceiptLineRequest {
  purchaseOrderLineId: number;
  quantity: number;
  unitCost?: number;
}

export interface CreateGoodsReceiptRequest {
  purchaseOrderId: number;
  notes?: string;
  lines: AddGoodsReceiptLineRequest[];
  postImmediately?: boolean;
}

export interface UpdateGoodsReceiptRequest {
  notes?: string;
  lines: AddGoodsReceiptLineRequest[];
}
