export function normalizeStatus(status: string): string {
  return status.toLowerCase().replace(/[^a-z]/g, '');
}

export function isSoDraftOrCancelled(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'draft' || s === 'cancelled';
}

export function isSoConfirmed(status: string): boolean {
  return normalizeStatus(status) === 'confirmed';
}

export function isSoPartialDelivery(status: string): boolean {
  return normalizeStatus(status) === 'partialdelivery';
}

export function canOpenDeliveryNote(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'confirmed' || s === 'partialdelivery' || s === 'delivered';
}

export function isDeliveryNotePosted(status: string): boolean {
  return normalizeStatus(status) === 'posted';
}

export function isDeliveryNoteDraft(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}
