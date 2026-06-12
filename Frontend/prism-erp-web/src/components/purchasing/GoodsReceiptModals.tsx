import type { GoodsReceiptDto, GoodsReceiptSummaryDto } from '../../services/types/purchasing.types';
import type { GoodsReceiptLineEditable } from '../../pages/purchasing/goodsReceiptFromPO';
import { isPoPartiallyReceived } from '../../pages/purchasing/goodsReceiptFromPO';

interface GoodsReceiptEditModalProps {
  isOpen: boolean;
  loading: boolean;
  goodsReceipt: GoodsReceiptDto | null;
  lines: GoodsReceiptLineEditable[];
  onClose: () => void;
  onLineChange: (index: number, quantityReceived: number) => void;
  onNotesChange: (notes: string) => void;
  onSave: () => void;
  onPost: () => void;
  formatCurrency: (value: number) => string;
}

export function GoodsReceiptEditModal({
  isOpen,
  loading,
  goodsReceipt,
  lines,
  onClose,
  onLineChange,
  onNotesChange,
  onSave,
  onPost,
  formatCurrency,
}: GoodsReceiptEditModalProps) {
  if (!isOpen) return null;

  const isDraft = goodsReceipt?.status === 'Draft';

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>Goods Receipt</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          {loading ? (
            <div className="loading">Loading goods receipt...</div>
          ) : goodsReceipt ? (
            <div className="order-detail">
              <div className="order-info-section">
                <h3>Receipt Information</h3>
                <div className="info-grid">
                  <div className="info-item">
                    <label>Receipt Number:</label>
                    <span>{goodsReceipt.receiptNumber}</span>
                  </div>
                  <div className="info-item">
                    <label>Purchase Order ID:</label>
                    <span>{goodsReceipt.purchaseOrderId}</span>
                  </div>
                  <div className="info-item">
                    <label>Status:</label>
                    <span className={`status ${goodsReceipt.status.toLowerCase()}`}>{goodsReceipt.status}</span>
                  </div>
                </div>
                <div className="form-group">
                  <label>Notes:</label>
                  <textarea
                    value={goodsReceipt.notes || ''}
                    onChange={(e) => onNotesChange(e.target.value)}
                    placeholder="Add notes..."
                    rows={3}
                    disabled={!isDraft}
                  />
                </div>
              </div>

              <div className="order-lines-section">
                <h3>Receipt Lines</h3>
                <table className="data-table editable-table">
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>Qty Ordered</th>
                      <th>Qty Remaining</th>
                      <th>Qty Received</th>
                      <th>Unit Cost</th>
                      <th>Line Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, index) => (
                      <tr key={line.id || index}>
                        <td>{line.productName}</td>
                        <td>{line.quantityOrdered}</td>
                        <td>{line.quantityRemaining}</td>
                        <td>
                          <input
                            type="number"
                            value={line.quantityReceived}
                            onChange={(e) => onLineChange(index, parseFloat(e.target.value) || 0)}
                            min="0"
                            max={line.quantityRemaining > 0 ? line.quantityRemaining : line.quantityOrdered}
                            step="0.01"
                            className="table-input"
                            disabled={!isDraft}
                          />
                        </td>
                        <td>{formatCurrency(line.unitCost)}</td>
                        <td className="line-total">{formatCurrency(line.lineTotal)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="order-totals-footer">
                <div className="totals-grid">
                  <div className="total-item total-amount-item">
                    <label>Total Amount:</label>
                    <span className="grand-total">
                      {formatCurrency(lines.reduce((sum, line) => sum + line.lineTotal, 0))}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          ) : null}
        </div>
        {goodsReceipt && (
          <div className="modal-footer">
            <button className="cancel-button" onClick={onClose}>Cancel</button>
            {goodsReceipt.id !== 0 && isDraft && (
              <button className="approve-button" onClick={onPost}>Post</button>
            )}
            {isDraft && (
              <button className="save-button" onClick={onSave}>Save</button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

interface GoodsReceiptSearchModalProps {
  isOpen: boolean;
  purchaseOrderId?: number;
  poStatus?: string;
  receipts: GoodsReceiptSummaryDto[];
  onClose: () => void;
  onSelect: (receiptId: number) => void;
  onCreateNew: () => void;
  formatDate: (date: string | Date | null | undefined) => string;
}

export function GoodsReceiptSearchModal({
  isOpen,
  purchaseOrderId,
  poStatus,
  receipts,
  onClose,
  onSelect,
  onCreateNew,
  formatDate,
}: GoodsReceiptSearchModalProps) {
  if (!isOpen) return null;

  const showCreateButton = poStatus ? isPoPartiallyReceived(poStatus) : false;

  return (
    <div className="modal-overlay">
      <div className="modal">
        <div className="modal-header">
          <h2>Goods Receipts for PO #{purchaseOrderId}</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <div className="data-table-container">
            <div className="table-header">
              <h2>Goods Receipt List</h2>
              {showCreateButton && (
                <button className="add-button" onClick={onCreateNew}>+ Create New Receipt</button>
              )}
            </div>
            <table className="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Receipt Number</th>
                  <th>Status</th>
                  <th>Posted Date</th>
                </tr>
              </thead>
              <tbody>
                {receipts.map((receipt) => (
                  <tr
                    key={receipt.id}
                    className="search-row"
                    style={{ cursor: 'pointer' }}
                    onClick={() => onSelect(receipt.id)}
                  >
                    <td>{receipt.id}</td>
                    <td>{receipt.receiptNumber}</td>
                    <td>
                      <span className={`status ${receipt.status.toLowerCase()}`}>
                        {receipt.status}
                      </span>
                    </td>
                    <td>{receipt.postedAt ? formatDate(receipt.postedAt) : 'N/A'}</td>
                  </tr>
                ))}
                {receipts.length === 0 && (
                  <tr>
                    <td colSpan={4} style={{ textAlign: 'center', padding: '20px' }}>
                      No goods receipts found for this purchase order
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
