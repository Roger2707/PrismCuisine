import { normalizeStatus } from './delivery/statusHelpers';

export function canApproveSalesOrder(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

export function canEditSalesOrder(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

/** Draft or Confirmed (no delivery started on SO). */
export function canCancelSalesOrder(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'draft' || s === 'confirmed';
}

export function isSalesOrderCancelled(status: string): boolean {
  return normalizeStatus(status) === 'cancelled';
}

export function isSalesOrderTerminal(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'cancelled' || s === 'delivered';
}
