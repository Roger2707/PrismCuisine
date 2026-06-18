import ProductSearch from '../ProductSearch';
import { formatCurrency } from '../../utils/formatters';
import type { OrderLineEditable } from '../../pages/purchasing/types';
import type { ProductDto } from '../../services/types/inventory.types';

interface PurchaseOrderLineTableProps {
  lines: OrderLineEditable[];
  readOnly: boolean;
  useProductSearch: boolean;
  showReceiptCols: boolean;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onRemoveLine: (index: number) => void;
}

export function PurchaseOrderLineTableHeader({
  showReceiptCols,
  showDiscountCols,
  showActions,
}: {
  showReceiptCols: boolean;
  showDiscountCols?: boolean;
  showActions: boolean;
}) {
  return (
    <thead>
      <tr>
        <th>Product</th>
        <th>Qty Ordered</th>
        {showReceiptCols && (
          <>
            <th>Qty Received</th>
            <th>Qty Remaining</th>
          </>
        )}
        <th>Unit Price</th>
        {showDiscountCols && <th>Discount %</th>}
        <th>Line Total</th>
        {showActions && <th>Actions</th>}
      </tr>
    </thead>
  );
}

export function PurchaseOrderLineTable({
  lines,
  readOnly,
  useProductSearch,
  showReceiptCols,
  onLineChange,
  onProductChange,
  onRemoveLine,
}: PurchaseOrderLineTableProps) {
  return (
    <>
      {lines.map((line, index) => (
        <tr key={line.id}>
          <td>
            {useProductSearch ? (
              <ProductSearch value={line.productData ?? null} onChange={(p) => onProductChange(index, p)} />
            ) : (
              line.productName
            )}
          </td>
          <td>
            <input
              type="number"
              value={line.quantityOrdered}
              onChange={(e) => onLineChange(index, 'quantityOrdered', parseFloat(e.target.value) || 0)}
              disabled={readOnly}
              min="0"
              step="0.01"
              className="table-input"
            />
          </td>
          {showReceiptCols && (
            <>
              <td>{line.quantityReceived}</td>
              <td>{line.quantityRemaining}</td>
            </>
          )}
          <td>
            <input
              type="number"
              value={line.unitPrice}
              onChange={(e) => onLineChange(index, 'unitPrice', parseFloat(e.target.value) || 0)}
              disabled={readOnly}
              min="0"
              step="0.01"
              className="table-input"
            />
          </td>
          <td className="line-total">{formatCurrency(line.lineTotal)}</td>
          {!readOnly && (
            <td>
              <button
                className="action-btn delete"
                onClick={() => onRemoveLine(index)}
                style={{ padding: '4px 8px', fontSize: '12px' }}
              >
                Remove
              </button>
            </td>
          )}
        </tr>
      ))}
    </>
  );
}
