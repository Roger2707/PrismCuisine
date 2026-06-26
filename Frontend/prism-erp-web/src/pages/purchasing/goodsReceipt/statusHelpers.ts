export function normalizeStatus(status: string): string {
  return status.toLowerCase().replace(/[^a-z]/g, '');
}

export function isPoDraftOrCancelled(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'draft' || s === 'cancelled';
}

export function isPoApproved(status: string): boolean {
  return normalizeStatus(status) === 'approved';
}

export function isPoPartiallyReceived(status: string): boolean {
  return normalizeStatus(status) === 'partiallyreceived';
}

export function isPoFullyReceived(status: string): boolean {
  return normalizeStatus(status) === 'received';
}

export function canOpenGoodsReceipt(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'approved' || s === 'partiallyreceived' || s === 'received';
}

export function isGoodsReceiptPosted(status: string): boolean {
  return normalizeStatus(status) === 'posted';
}

export function isGoodsReceiptDraft(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

export function isGoodsReceiptCancelled(status: string): boolean {
  return normalizeStatus(status) === 'cancelled';
}

/** Backend only allows cancel on Posted receipts (reverses inventory). */
export function canCancelGoodsReceipt(status: string): boolean {
  return isGoodsReceiptPosted(status);
}
