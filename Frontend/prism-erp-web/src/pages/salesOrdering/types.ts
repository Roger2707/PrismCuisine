import type { SalesOrderSummaryDto, SalesOrderDto, CustomerDto } from '../../services/types/salesOrdering.types';
import type { ProductDto } from '../../services/types/inventory.types';

export interface SalesOrder extends SalesOrderSummaryDto {}

export interface OrderDetail extends SalesOrderDto {
  customerData?: CustomerDto;
}

export interface OrderLineEditable {
  id: number;
  productId: number;
  productName: string;
  productData?: ProductDto | null;
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

export function createEmptyLine(): OrderLineEditable {
  return {
    id: Date.now(),
    productId: 0,
    productName: '',
    productData: null,
    quantityOrdered: 0,
    quantityDelivered: 0,
    quantityRemaining: 0,
    unitPrice: 0,
    discountPercent: 0,
    vatRate: 10,
    discountAmount: 0,
    vatAmount: 0,
    lineTotal: 0,
  };
}
