import CustomerSearch from '../CustomerSearch';
import { formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';
import { formatOrderInvoicingStatus } from '../../services/types/finance.types';
import type { CustomerDto } from '../../services/types/salesOrdering.types';
import type { OrderDetail, OrderLineEditable } from '../../pages/salesOrdering/types';
import type { ProductDto } from '../../services/types/inventory.types';
import { SalesOrderLineTable, SalesOrderLineTableHeader } from './SalesOrderLineTable';
import { SalesOrderTotals } from './SalesOrderTotals';

interface SalesOrderEditFormProps {
  orderDetail: OrderDetail;
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  isReadOnly: boolean;
  onOrderDetailChange: (detail: OrderDetail) => void;
  onCustomerChange: (customer: CustomerDto | null) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

export function SalesOrderEditForm({
  orderDetail,
  editableLines,
  fieldErrors,
  isReadOnly,
  onOrderDetailChange,
  onCustomerChange,
  onLineChange,
  onProductChange,
  onAddLine,
  onRemoveLine,
}: SalesOrderEditFormProps) {
  return (
    <div className="order-detail">
      <div className="order-info-section">
        <h3>Order Information</h3>
        <div className="info-grid">
          <div className="info-item"><label>Order Number:</label><span>{orderDetail.orderNumber}</span></div>
          <div className="info-item">
            <label>Customer:</label>
            <CustomerSearch
              value={orderDetail.customerData || null}
              onChange={onCustomerChange}
              disabled={isReadOnly}
              hasError={!!fieldErrors.customerId}
            />
          </div>
          <div className="info-item"><label>Order Date:</label><span>{formatDate(orderDetail.orderDate)}</span></div>
          <div className="info-item">
            <label>Status:</label>
            <StatusBadge status={orderDetail.status} />
          </div>
          <div className="info-item">
            <label>Invoice Status:</label>
            <StatusBadge
              status={orderDetail.invoiceStatus || 'NotInvoiced'}
              label={formatOrderInvoicingStatus(orderDetail.invoiceStatus || 'NotInvoiced')}
            />
          </div>
        </div>
        <div className="form-group">
          <label>Notes:</label>
          <textarea
            value={orderDetail.notes || ''}
            onChange={(e) => onOrderDetailChange({ ...orderDetail, notes: e.target.value })}
            rows={3}
            disabled={isReadOnly}
          />
        </div>
      </div>
      <div className="order-lines-section">
        <h3>Order Lines</h3>
        <table className="data-table editable-table">
          <SalesOrderLineTableHeader showDeliveryCols showActions={!isReadOnly} />
          <tbody>
            <SalesOrderLineTable
              lines={editableLines}
              readOnly={isReadOnly}
              useProductSearch={false}
              onLineChange={onLineChange}
              onProductChange={onProductChange}
              onRemoveLine={onRemoveLine}
            />
          </tbody>
        </table>
        {!isReadOnly && (
          <button className="add-button" onClick={onAddLine} style={{ marginTop: '10px' }}>+ Add Line</button>
        )}
      </div>
      <SalesOrderTotals orderDetail={orderDetail} />
    </div>
  );
}
