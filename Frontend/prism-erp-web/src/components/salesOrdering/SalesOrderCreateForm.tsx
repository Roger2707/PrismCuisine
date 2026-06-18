import CustomerSearch from '../CustomerSearch';
import type { CustomerDto } from '../../services/types/salesOrdering.types';
import type { OrderLineEditable } from '../../pages/salesOrdering/types';
import type { ProductDto } from '../../services/types/inventory.types';
import { SalesOrderLineTable, SalesOrderLineTableHeader } from './SalesOrderLineTable';

interface SalesOrderCreateFormProps {
  createCustomer: CustomerDto | null;
  createNotes: string;
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  onCustomerChange: (customer: CustomerDto | null) => void;
  onNotesChange: (notes: string) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

export function SalesOrderCreateForm({
  createCustomer,
  createNotes,
  editableLines,
  fieldErrors,
  onCustomerChange,
  onNotesChange,
  onLineChange,
  onProductChange,
  onAddLine,
  onRemoveLine,
}: SalesOrderCreateFormProps) {
  return (
    <div className="order-detail">
      <div className="order-info-section">
        <h3>New Sales Order</h3>
        <div className="info-grid">
          <div className="info-item">
            <label>Customer:</label>
            <CustomerSearch value={createCustomer} onChange={onCustomerChange} hasError={!!fieldErrors.customerId} />
          </div>
        </div>
        <div className="form-group">
          <label>Notes:</label>
          <textarea value={createNotes} onChange={(e) => onNotesChange(e.target.value)} rows={3} />
        </div>
      </div>
      <div className="order-lines-section">
        <h3>Order Lines</h3>
        <table className="data-table editable-table">
          <SalesOrderLineTableHeader showDeliveryCols={false} showActions />
          <tbody>
            <SalesOrderLineTable
              lines={editableLines}
              readOnly={false}
              useProductSearch
              onLineChange={onLineChange}
              onProductChange={onProductChange}
              onRemoveLine={onRemoveLine}
            />
          </tbody>
        </table>
        <button className="add-button" onClick={onAddLine} style={{ marginTop: '10px' }}>+ Add Line</button>
      </div>
    </div>
  );
}
