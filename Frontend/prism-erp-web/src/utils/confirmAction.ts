/** Returns true when the user confirms the action (Yes). */
export function confirmAction(message: string): boolean {
  return window.confirm(message);
}

export const ConfirmMessages = {
  createPurchaseOrder: 'Create this purchase order?',
  savePurchaseOrder: 'Save changes to this purchase order?',
  approvePurchaseOrder: 'Approve this purchase order?',
  cancelPurchaseOrder: 'Cancel this purchase order? Draft goods receipts will be removed.',

  createSalesOrder: 'Create this sales order?',
  saveSalesOrder: 'Save changes to this sales order?',
  approveSalesOrder: 'Approve this sales order and reserve inventory?',
  cancelSalesOrder: 'Cancel this sales order? Draft delivery notes will be removed and reservations released.',

  saveGoodsReceipt: 'Save this goods receipt?',
  postGoodsReceipt: 'Post this goods receipt? Inventory will be updated.',
  cancelGoodsReceipt:
    'Cancel this posted goods receipt? Received stock will be reversed and linked invoice cancelled if any.',

  saveDeliveryNote: 'Save this delivery note?',
  postDeliveryNote: 'Post this delivery note? Stock will be issued and a sales invoice created.',
  cancelDeliveryNote:
    'Cancel this posted delivery note? Issued stock will be returned and the sales invoice cancelled.',

  createPurchaseInvoice: 'Create purchase invoice for this goods receipt?',
} as const;
