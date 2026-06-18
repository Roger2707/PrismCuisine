import type { PurchaseOrderSummaryDto, PurchaseOrderDto, SupplierDto } from '../../services/types/purchasing.types';
import type { ProductDto } from '../../services/types/inventory.types';

export interface PurchaseOrder extends PurchaseOrderSummaryDto {}

export interface OrderDetail extends PurchaseOrderDto {
  supplierData?: SupplierDto;
  supplierName?: string;
  totalAmount?: number;
}

export interface OrderLineEditable {
  id: number;
  productId: number;
  productName: string;
  productData?: ProductDto | null;
  quantityOrdered: number;
  quantityReceived: number;
  quantityRemaining: number;
  unitPrice: number;
  lineTotal: number;
}

export function createEmptyLine(): OrderLineEditable {
  return {
    id: Date.now(),
    productId: 0,
    productName: '',
    productData: null,
    quantityOrdered: 0,
    quantityReceived: 0,
    quantityRemaining: 0,
    unitPrice: 0,
    lineTotal: 0,
  };
}

export function mapPoLinesFromDto(
  lines: PurchaseOrderDto['lines'],
): OrderLineEditable[] {
  return lines.map((line) => ({
    ...line,
    productName: `Product ${line.productId}`,
    lineTotal: line.quantityOrdered * line.unitPrice,
  }));
}

export function buildMockOrderDetail(order: PurchaseOrder, mockLines: OrderLineEditable[]): OrderDetail {
  return {
    id: order.id,
    orderNumber: order.orderNumber,
    supplierId: order.supplierId,
    warehouseId: order.warehouseId,
    status: order.status,
    invoiceStatus: order.invoiceStatus || 'NotInvoiced',
    approvedAt: order.approvedAt,
    amendedFromPurchaseOrderId: order.amendedFromPurchaseOrderId,
    notes: 'Sample notes for the order',
    lines: mockLines,
    supplierName: 'Supplier Name',
  };
}

export const MOCK_PO_LINES: OrderLineEditable[] = [
  {
    id: 1,
    productId: 1,
    productName: 'Product A',
    quantityOrdered: 10,
    quantityReceived: 0,
    quantityRemaining: 10,
    unitPrice: 100000,
    lineTotal: 1000000,
  },
  {
    id: 2,
    productId: 2,
    productName: 'Product B',
    quantityOrdered: 5,
    quantityReceived: 0,
    quantityRemaining: 5,
    unitPrice: 200000,
    lineTotal: 1000000,
  },
];
