import { formatCurrency, formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';
import type { SalesOrder } from '../../pages/salesOrdering/types';

interface SalesOrderListProps {
  orders: SalesOrder[];
  onEdit: (order: SalesOrder) => void;
  onApprove: (id: number) => void;
  onCancel: (id: number) => void;
}

export function SalesOrderList({ orders, onEdit, onApprove, onCancel }: SalesOrderListProps) {
  return (
    <div className="data-table-container">
      <table className="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Order Number</th>
            <th>Customer</th>
            <th>Total Amount</th>
            <th>Status</th>
            <th>Order Date</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr key={order.id}>
              <td>{order.id}</td>
              <td>{order.orderNumber}</td>
              <td>{order.customerName}</td>
              <td>{formatCurrency(order.totalAmount)}</td>
              <td><StatusBadge status={order.status} /></td>
              <td>{formatDate(order.orderDate)}</td>
              <td>
                <button className="action-btn edit" onClick={() => onEdit(order)}>Edit</button>
                <button
                  className="action-btn approve"
                  onClick={() => onApprove(order.id)}
                  disabled={order.status !== 'Draft'}
                >
                  Approve
                </button>
                <button className="action-btn delete" onClick={() => onCancel(order.id)}>Cancel</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
