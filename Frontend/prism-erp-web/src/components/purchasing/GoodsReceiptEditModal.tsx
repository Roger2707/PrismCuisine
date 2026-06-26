import type { GoodsReceiptDto } from '../../services/types/purchasing.types';
import type { GoodsReceiptLineEditable } from '../../pages/purchasing/goodsReceipt/types';
import { formatCurrency } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';
import { LoadingButton } from '../LoadingButton';

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
  onCancelReceipt?: () => void;
  onViewInvoice?: () => void;
  onCreateInvoice?: () => void;
  showViewInvoice?: boolean;
  showCreateInvoice?: boolean;
  showCancelReceipt?: boolean;
  creatingInvoice?: boolean;
  saving?: boolean;
  posting?: boolean;
  cancelling?: boolean;
  viewingInvoice?: boolean;
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
  onCancelReceipt,
  onViewInvoice,
  onCreateInvoice,
  showViewInvoice,
  showCreateInvoice,
  showCancelReceipt,
  creatingInvoice,
  saving,
  posting,
  cancelling,
  viewingInvoice,
}: GoodsReceiptEditModalProps) {
  if (!isOpen) return null;

  const isDraft = goodsReceipt?.status === 'Draft';
  const busy = saving || posting || cancelling || creatingInvoice || viewingInvoice;

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>Goods Receipt</h2>
          <button className="close-button" onClick={onClose} disabled={busy}>×</button>
        </div>
        <div className="modal-body">
          {loading ? (
            <div className="loading">Loading goods receipt...</div>
          ) : goodsReceipt ? (
            <div className="order-detail">
              <div className="order-info-section">
                <h3>Receipt Information</h3>
                <div className="info-grid">
                  <div className="info-item"><label>Receipt Number:</label><span>{goodsReceipt.receiptNumber}</span></div>
                  <div className="info-item"><label>Purchase Order ID:</label><span>{goodsReceipt.purchaseOrderId}</span></div>
                  <div className="info-item">
                    <label>Status:</label>
                    <StatusBadge status={goodsReceipt.status} />
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
            <LoadingButton variant="secondary" onClick={onClose} disabled={busy}>Close</LoadingButton>
            {showCreateInvoice && onCreateInvoice && (
              <LoadingButton variant="action" onClick={onCreateInvoice} loading={creatingInvoice} loadingText="Creating..." disabled={busy}>
                Create Invoice
              </LoadingButton>
            )}
            {showViewInvoice && onViewInvoice && (
              <LoadingButton variant="action" onClick={onViewInvoice} loading={viewingInvoice} loadingText="Loading..." disabled={busy}>
                View Invoice
              </LoadingButton>
            )}
            {showCancelReceipt && onCancelReceipt && (
              <LoadingButton variant="danger" onClick={onCancelReceipt} loading={cancelling} loadingText="Cancelling..." disabled={busy}>
                Cancel Receipt
              </LoadingButton>
            )}
            {goodsReceipt.id !== 0 && isDraft && (
              <LoadingButton variant="approve" onClick={onPost} loading={posting} loadingText="Posting..." disabled={busy}>Post</LoadingButton>
            )}
            {isDraft && (
              <LoadingButton variant="primary" onClick={onSave} loading={saving} loadingText="Saving..." disabled={busy}>Save</LoadingButton>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
