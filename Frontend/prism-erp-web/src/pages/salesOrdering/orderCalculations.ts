import type { OrderLineEditable } from './types';

export function calculateLineTotals(lines: OrderLineEditable[]): OrderLineEditable[] {
  return lines.map((line) => {
    const gross = line.unitPrice * line.quantityOrdered;
    const discountAmount = gross * (line.discountPercent / 100);
    const afterDiscount = gross - discountAmount;
    const vatAmount = afterDiscount * (line.vatRate / 100);
    return {
      ...line,
      discountAmount,
      vatAmount,
      lineTotal: afterDiscount + vatAmount,
    };
  });
}

export function calculateOrderTotals(lines: OrderLineEditable[]) {
  const subTotal = lines.reduce((sum, line) => sum + line.unitPrice * line.quantityOrdered, 0);
  const totalDiscount = lines.reduce((sum, line) => sum + line.discountAmount, 0);
  const totalVAT = lines.reduce((sum, line) => sum + line.vatAmount, 0);
  const totalAmount = lines.reduce((sum, line) => sum + line.lineTotal, 0);
  return { subTotal, totalDiscount, totalVAT, totalAmount };
}
