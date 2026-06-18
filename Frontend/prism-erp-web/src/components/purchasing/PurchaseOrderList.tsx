import { formatCurrency } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';
import type { PurchaseOrder } from '../../pages/purchasing/types';

interface PurchaseOrderListProps {
  orders: PurchaseOrder[];
  onEdit: (order: PurchaseOrder) => void;
  onApprove: (id: number) => void;
  onCancel: (id: number) => void;
}

export function PurchaseOrderList({ orders, onEdit, onApprove, onCancel }: PurchaseOrderListProps) {
  return (
    <table className="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Order Number</th>
          <th>Supplier</th>
          <th>Total Amount</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <tr key={order.id}>
            <td>{order.id}</td>
            <td>{order.orderNumber}</td>
            <td>{order.supplierId}</td>
            <td>{formatCurrency(order.totalAmount)}</td>
            <td><StatusBadge status={order.status} /></td>
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
  );
}
