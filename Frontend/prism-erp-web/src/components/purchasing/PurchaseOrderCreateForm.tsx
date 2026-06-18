import SupplierSearch from '../SupplierSearch';
import type { SupplierDto } from '../../services/types/purchasing.types';
import type { OrderLineEditable } from '../../pages/purchasing/types';
import type { ProductDto } from '../../services/types/inventory.types';
import { PurchaseOrderLineTable, PurchaseOrderLineTableHeader } from './PurchaseOrderLineTable';
import { PurchaseOrderTotals } from './PurchaseOrderTotals';
import { sumLineTotals } from '../../pages/purchasing/orderCalculations';

interface PurchaseOrderCreateFormProps {
  createSupplier: SupplierDto | null;
  createNotes: string;
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  onSupplierChange: (supplier: SupplierDto | null) => void;
  onNotesChange: (notes: string) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

export function PurchaseOrderCreateForm({
  createSupplier,
  createNotes,
  editableLines,
  fieldErrors,
  onSupplierChange,
  onNotesChange,
  onLineChange,
  onProductChange,
  onAddLine,
  onRemoveLine,
}: PurchaseOrderCreateFormProps) {
  return (
    <div className="order-detail">
      <div className="order-info-section">
        <h3>New Purchase Order</h3>
        <div className="info-grid">
          <div className="info-item">
            <label>Supplier:</label>
            <SupplierSearch value={createSupplier} onChange={onSupplierChange} hasError={!!fieldErrors.supplierId} />
          </div>
        </div>
        <div className="form-group">
          <label>Notes:</label>
          <textarea value={createNotes} onChange={(e) => onNotesChange(e.target.value)} placeholder="Add notes..." rows={3} />
        </div>
      </div>
      <div className="order-lines-section">
        <h3>Order Lines</h3>
        <table className="data-table editable-table">
          <PurchaseOrderLineTableHeader showReceiptCols={false} showActions />
          <tbody>
            <PurchaseOrderLineTable
              lines={editableLines}
              readOnly={false}
              useProductSearch
              showReceiptCols={false}
              onLineChange={onLineChange}
              onProductChange={onProductChange}
              onRemoveLine={onRemoveLine}
            />
          </tbody>
        </table>
        <button className="add-button" onClick={onAddLine} style={{ marginTop: '10px' }}>+ Add Line</button>
      </div>
      <PurchaseOrderTotals totalAmount={sumLineTotals(editableLines)} />
    </div>
  );
}
