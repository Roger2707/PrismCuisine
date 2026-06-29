import ProductSearch from '../ProductSearch';
import { formatCurrency, formatProductLabel } from '../../utils/formatters';
import type { OrderLineEditable } from '../../pages/salesOrdering/types';
import type { ProductDto } from '../../services/types/inventory.types';

interface SalesOrderLineTableProps {
  lines: OrderLineEditable[];
  readOnly: boolean;
  useProductSearch: boolean;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onRemoveLine: (index: number) => void;
}

export function SalesOrderLineTable({
  lines,
  readOnly,
  useProductSearch,
  onLineChange,
  onProductChange,
  onRemoveLine,
}: SalesOrderLineTableProps) {
  return (
    <>
      {lines.map((line, index) => (
        <tr key={line.id}>
          <td>
            {useProductSearch ? (
              <ProductSearch value={line.productData ?? null} onChange={(p) => onProductChange(index, p)} />
            ) : (
              formatProductLabel(line.productName, line.productId)
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
          {!useProductSearch && (
            <>
              <td>{line.quantityDelivered}</td>
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
          <td>
            <input
              type="number"
              value={line.discountPercent}
              onChange={(e) => onLineChange(index, 'discountPercent', parseFloat(e.target.value) || 0)}
              disabled={readOnly}
              min="0"
              max="100"
              step="0.01"
              className="table-input"
            />
          </td>
          <td>
            <input
              type="number"
              value={line.vatRate}
              onChange={(e) => onLineChange(index, 'vatRate', parseFloat(e.target.value) || 0)}
              disabled={readOnly}
              min="0"
              max="100"
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

export function SalesOrderLineTableHeader({ showDeliveryCols, showActions }: { showDeliveryCols: boolean; showActions: boolean }) {
  return (
    <thead>
      <tr>
        <th>Product</th>
        <th>Qty Ordered</th>
        {showDeliveryCols && (
          <>
            <th>Qty Delivered</th>
            <th>Qty Remaining</th>
          </>
        )}
        <th>Unit Price</th>
        <th>Discount %</th>
        <th>VAT %</th>
        <th>Line Total</th>
        {showActions && <th>Actions</th>}
      </tr>
    </thead>
  );
}
