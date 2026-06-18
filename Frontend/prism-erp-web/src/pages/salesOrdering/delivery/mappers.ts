import type { SalesOrderDto, SalesOrderLineDto, DeliveryNoteDto } from '../../../services/types/salesOrdering.types';
import type { DeliveryNoteLineEditable } from './types';

export function mapSoLineToDnLine(
  line: SalesOrderLineDto,
  quantityDelivered: number,
  lineId = 0,
): DeliveryNoteLineEditable {
  return {
    id: lineId,
    salesOrderLineId: line.id,
    productId: line.productId,
    productName: line.productName,
    quantityOrdered: line.quantityOrdered,
    quantityRemaining: line.quantityRemaining,
    quantityDelivered,
  };
}

export function mapDeliveryLineToEditable(
  deliveryLine: DeliveryNoteDto['lines'][number],
  soLine?: SalesOrderLineDto,
): DeliveryNoteLineEditable {
  return {
    id: deliveryLine.id,
    salesOrderLineId: deliveryLine.salesOrderLineId,
    productId: deliveryLine.productId,
    productName: deliveryLine.productName,
    quantityOrdered: soLine?.quantityOrdered ?? 0,
    quantityRemaining: soLine?.quantityRemaining ?? 0,
    quantityDelivered: deliveryLine.quantityDelivered,
  };
}

export function buildNewDraftDelivery(salesOrder: SalesOrderDto): DeliveryNoteDto {
  return {
    id: 0,
    deliveryNumber: `DN-${new Date().getFullYear()}-${String(salesOrder.id).padStart(4, '0')}`,
    salesOrderId: salesOrder.id,
    customerId: salesOrder.customerId,
    customerName: salesOrder.customerName,
    orderNumber: salesOrder.orderNumber,
    deliveryDate: new Date().toISOString(),
    status: 'Draft',
    notes: '',
    lines: [],
  };
}

export function buildLinesForSave(lines: DeliveryNoteLineEditable[]) {
  return lines
    .filter((line) => line.quantityDelivered > 0)
    .map((line) => ({
      salesOrderLineId: line.salesOrderLineId,
      quantityDelivered: line.quantityDelivered,
    }));
}
