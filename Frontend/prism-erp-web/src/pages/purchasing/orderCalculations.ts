import type { OrderLineEditable } from './types';

export function calculateLineTotals(lines: OrderLineEditable[]): OrderLineEditable[] {
  return lines.map((line) => ({
    ...line,
    lineTotal: line.quantityOrdered * line.unitPrice,
  }));
}

export function calculateOrderTotals(lines: OrderLineEditable[]) {
  const totalAmount = lines.reduce((sum, line) => sum + line.lineTotal, 0);
  return { totalAmount };
}

export function sumLineTotals(lines: OrderLineEditable[]): number {
  return lines.reduce((sum, line) => sum + line.lineTotal, 0);
}
