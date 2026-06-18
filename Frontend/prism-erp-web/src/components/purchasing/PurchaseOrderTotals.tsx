import { formatCurrency } from '../../utils/formatters';

interface PurchaseOrderTotalsProps {
  totalAmount: number;
}

export function PurchaseOrderTotals({ totalAmount }: PurchaseOrderTotalsProps) {
  return (
    <div className="order-totals-footer">
      <div className="totals-grid">
        <div className="total-item total-amount-item">
          <label>Total Amount:</label>
          <span className="grand-total">{formatCurrency(totalAmount)}</span>
        </div>
      </div>
    </div>
  );
}
