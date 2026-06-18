export interface GoodsReceiptLineEditable {
  id: number;
  purchaseOrderLineId: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityRemaining: number;
  quantityReceived: number;
  unitCost: number;
  lineTotal: number;
}
