import { formatCurrency } from '../../utils/formatters';
import type { OrderDetail } from '../../pages/salesOrdering/types';

interface SalesOrderTotalsProps {
  orderDetail: OrderDetail;
}

export function SalesOrderTotals({ orderDetail }: SalesOrderTotalsProps) {
  return (
    <div className="order-totals-footer">
      <div className="totals-grid">
        <div className="total-item">
          <label>Sub Total:</label>
          <span>{formatCurrency(orderDetail.subTotal)}</span>
        </div>
        <div className="total-item">
          <label>Total Discount:</label>
          <span className="discount-value">-{formatCurrency(orderDetail.totalDiscount)}</span>
        </div>
        <div className="total-item">
          <label>Total VAT:</label>
          <span>{formatCurrency(orderDetail.totalVAT)}</span>
        </div>
        <div className="total-item total-amount-item">
          <label>Total Amount:</label>
          <span className="grand-total">{formatCurrency(orderDetail.totalAmount)}</span>
        </div>
      </div>
    </div>
  );
}
