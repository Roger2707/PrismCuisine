import type { DeliveryNoteDto } from '../../services/types/salesOrdering.types';
import type { DeliveryNoteLineEditable } from '../../pages/salesOrdering/delivery/types';
import { StatusBadge } from '../../utils/statusBadge';
import { LoadingButton } from '../LoadingButton';
import { formatDate } from '../../utils/formatters';

interface DeliveryNoteEditModalProps {
  isOpen: boolean;
  loading: boolean;
  deliveryNote: DeliveryNoteDto | null;
  lines: DeliveryNoteLineEditable[];
  onClose: () => void;
  onLineChange: (index: number, quantityDelivered: number) => void;
  onNotesChange: (notes: string) => void;
  onSave: () => void;
  onPost: () => void;
  onViewInvoice?: () => void;
  showViewInvoice?: boolean;
  saving?: boolean;
  posting?: boolean;
  viewingInvoice?: boolean;
}

export function DeliveryNoteEditModal({
  isOpen,
  loading,
  deliveryNote,
  lines,
  onClose,
  onLineChange,
  onNotesChange,
  onSave,
  onPost,
  onViewInvoice,
  showViewInvoice,
  saving,
  posting,
  viewingInvoice,
}: DeliveryNoteEditModalProps) {
  if (!isOpen) return null;

  const isDraft = deliveryNote?.status === 'Draft';

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>Delivery Note</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          {loading ? (
            <div className="loading">Loading delivery note...</div>
          ) : deliveryNote ? (
            <div className="order-detail">
              <div className="order-info-section">
                <h3>Delivery Information</h3>
                <div className="info-grid">
                  <div className="info-item"><label>Delivery Number:</label><span>{deliveryNote.deliveryNumber}</span></div>
                  <div className="info-item"><label>Sales Order:</label><span>{deliveryNote.orderNumber}</span></div>
                  <div className="info-item"><label>Customer:</label><span>{deliveryNote.customerName}</span></div>
                  <div className="info-item">
                    <label>Status:</label>
                    <StatusBadge status={deliveryNote.status} />
                  </div>
                  <div className="info-item"><label>Delivery Date:</label><span>{formatDate(deliveryNote.deliveryDate)}</span></div>
                </div>
                <div className="form-group">
                  <label>Notes:</label>
                  <textarea
                    value={deliveryNote.notes || ''}
                    onChange={(e) => onNotesChange(e.target.value)}
                    placeholder="Add notes..."
                    rows={3}
                    disabled={!isDraft}
                  />
                </div>
              </div>
              <div className="order-lines-section">
                <h3>Delivery Lines</h3>
                <table className="data-table editable-table">
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>Qty Ordered</th>
                      <th>Qty Remaining</th>
                      <th>Qty Delivered</th>
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
                            value={line.quantityDelivered}
                            onChange={(e) => onLineChange(index, parseFloat(e.target.value) || 0)}
                            min="0"
                            max={line.quantityRemaining > 0 ? line.quantityRemaining : line.quantityOrdered}
                            step="0.01"
                            className="table-input"
                            disabled={!isDraft}
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : null}
        </div>
        {deliveryNote && (
          <div className="modal-footer">
            <LoadingButton variant="secondary" onClick={onClose} disabled={saving || posting}>Cancel</LoadingButton>
            {showViewInvoice && onViewInvoice && (
              <LoadingButton variant="action" onClick={onViewInvoice} loading={viewingInvoice} loadingText="Loading...">
                View Invoice
              </LoadingButton>
            )}
            {deliveryNote.id !== 0 && isDraft && (
              <LoadingButton variant="approve" onClick={onPost} loading={posting} loadingText="Posting...">Post</LoadingButton>
            )}
            {isDraft && (
              <LoadingButton variant="primary" onClick={onSave} loading={saving} loadingText="Saving...">Save</LoadingButton>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
