import { normalizeStatus } from './goodsReceipt/statusHelpers';

export function canApprovePurchaseOrder(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

export function canEditPurchaseOrder(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

/** Draft or Approved (no partial/full receive on PO header). */
export function canCancelPurchaseOrder(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'draft' || s === 'approved';
}

export function isPurchaseOrderCancelled(status: string): boolean {
  return normalizeStatus(status) === 'cancelled';
}

export function isPurchaseOrderTerminal(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'cancelled' || s === 'received';
}
