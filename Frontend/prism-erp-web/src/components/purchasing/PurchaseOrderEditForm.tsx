import SupplierSearch from '../SupplierSearch';
import { formatOrderInvoicingStatus } from '../../services/types/finance.types';
import { StatusBadge } from '../../utils/statusBadge';
import type { SupplierDto } from '../../services/types/purchasing.types';
import type { OrderDetail, OrderLineEditable } from '../../pages/purchasing/types';
import type { ProductDto } from '../../services/types/inventory.types';
import { PurchaseOrderLineTable, PurchaseOrderLineTableHeader } from './PurchaseOrderLineTable';
import { PurchaseOrderTotals } from './PurchaseOrderTotals';

interface PurchaseOrderEditFormProps {
  orderDetail: OrderDetail;
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  isReadOnly: boolean;
  onOrderDetailChange: (detail: OrderDetail) => void;
  onSupplierChange: (supplier: SupplierDto | null) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

export function PurchaseOrderEditForm({
  orderDetail,
  editableLines,
  fieldErrors,
  isReadOnly,
  onOrderDetailChange,
  onSupplierChange,
  onLineChange,
  onProductChange,
  onAddLine,
  onRemoveLine,
}: PurchaseOrderEditFormProps) {
  return (
    <div className="order-detail">
      <div className="order-info-section">
        <h3>Order Information</h3>
        <div className="info-grid">
          <div className="info-item"><label>Order Number:</label><span>{orderDetail.orderNumber}</span></div>
          <div className="info-item">
            <label>Supplier:</label>
            <SupplierSearch
              value={orderDetail.supplierData || null}
              onChange={onSupplierChange}
              disabled={isReadOnly}
              hasError={!!fieldErrors.supplierId}
            />
            {fieldErrors.supplierId && <span className="field-error">{fieldErrors.supplierId}</span>}
          </div>
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
            placeholder="Add notes..."
            rows={3}
            disabled={isReadOnly}
          />
        </div>
      </div>
      <div className="order-lines-section">
        <h3>Order Lines</h3>
        <table className="data-table editable-table">
          <PurchaseOrderLineTableHeader showReceiptCols showActions={!isReadOnly} />
          <tbody>
            <PurchaseOrderLineTable
              lines={editableLines}
              readOnly={isReadOnly}
              useProductSearch={false}
              showReceiptCols
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
      <PurchaseOrderTotals totalAmount={orderDetail.totalAmount || 0} />
    </div>
  );
}
