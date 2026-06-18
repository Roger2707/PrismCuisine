import type { PurchaseOrderLineDto, GoodsReceiptDto } from '../../../services/types/purchasing.types';
import type { GoodsReceiptLineEditable } from './types';

export function mapPoLineToGrLine(
  line: PurchaseOrderLineDto,
  quantityReceived: number,
  lineId = 0,
): GoodsReceiptLineEditable {
  return {
    id: lineId,
    purchaseOrderLineId: line.id,
    productId: line.productId,
    productName: `Product ${line.productId}`,
    quantityOrdered: line.quantityOrdered,
    quantityRemaining: line.quantityRemaining,
    quantityReceived,
    unitCost: line.unitPrice,
    lineTotal: quantityReceived * line.unitPrice,
  };
}

export function mapReceiptLineToEditable(
  receiptLine: GoodsReceiptDto['lines'][number],
  poLine?: PurchaseOrderLineDto,
): GoodsReceiptLineEditable {
  return {
    id: receiptLine.id,
    purchaseOrderLineId: receiptLine.purchaseOrderLineId,
    productId: receiptLine.productId,
    productName: `Product ${receiptLine.productId}`,
    quantityOrdered: poLine?.quantityOrdered ?? 0,
    quantityRemaining: poLine?.quantityRemaining ?? 0,
    quantityReceived: receiptLine.quantity,
    unitCost: receiptLine.unitCost,
    lineTotal: receiptLine.quantity * receiptLine.unitCost,
  };
}

export function buildNewDraftReceipt(purchaseOrderId: number, suffix = ''): GoodsReceiptDto {
  const base = `GR-${new Date().getFullYear()}-${String(purchaseOrderId).padStart(4, '0')}`;
  return {
    id: 0,
    receiptNumber: suffix ? `${base}-${suffix}` : base,
    purchaseOrderId,
    status: 'Draft',
    postedAt: undefined,
    notes: '',
    lines: [],
  };
}

export function buildLinesForSave(lines: GoodsReceiptLineEditable[]) {
  return lines
    .filter((line) => line.quantityReceived > 0)
    .map((line) => ({
      purchaseOrderLineId: line.purchaseOrderLineId,
      quantity: line.quantityReceived,
      unitCost: line.unitCost,
    }));
}
