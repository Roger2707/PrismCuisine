export interface DeliveryNoteLineEditable {
  id: number;
  salesOrderLineId: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityRemaining: number;
  quantityDelivered: number;
}
