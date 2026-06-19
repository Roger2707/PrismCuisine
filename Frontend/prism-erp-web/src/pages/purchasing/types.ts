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
