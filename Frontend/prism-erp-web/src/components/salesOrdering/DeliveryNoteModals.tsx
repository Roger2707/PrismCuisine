import type { DeliveryNoteDto, DeliveryNoteSummaryDto } from '../../services/types/salesOrdering.types';
import type { DeliveryNoteLineEditable } from '../../pages/salesOrdering/deliveryNoteFromSO';
import { isSoPartialDelivery } from '../../pages/salesOrdering/deliveryNoteFromSO';

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
  formatDate: (date: string | Date | null | undefined) => string;
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
  formatDate,
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
                  <div className="info-item">
                    <label>Delivery Number:</label>
                    <span>{deliveryNote.deliveryNumber}</span>
                  </div>
                  <div className="info-item">
                    <label>Sales Order:</label>
                    <span>{deliveryNote.orderNumber}</span>
                  </div>
                  <div className="info-item">
                    <label>Customer:</label>
                    <span>{deliveryNote.customerName}</span>
                  </div>
                  <div className="info-item">
                    <label>Status:</label>
                    <span className={`status ${deliveryNote.status.toLowerCase()}`}>{deliveryNote.status}</span>
                  </div>
                  <div className="info-item">
                    <label>Delivery Date:</label>
                    <span>{formatDate(deliveryNote.deliveryDate)}</span>
                  </div>
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
            <button className="cancel-button" onClick={onClose}>Cancel</button>
            {deliveryNote.id !== 0 && isDraft && (
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

interface DeliveryNoteSearchModalProps {
  isOpen: boolean;
  salesOrderId?: number;
  soStatus?: string;
  notes: DeliveryNoteSummaryDto[];
  onClose: () => void;
  onSelect: (deliveryId: number) => void;
  onCreateNew: () => void;
  formatDate: (date: string | Date | null | undefined) => string;
}

export function DeliveryNoteSearchModal({
  isOpen,
  salesOrderId,
  soStatus,
  notes,
  onClose,
  onSelect,
  onCreateNew,
  formatDate,
}: DeliveryNoteSearchModalProps) {
  if (!isOpen) return null;

  const showCreateButton = soStatus ? isSoPartialDelivery(soStatus) : false;

  return (
    <div className="modal-overlay">
      <div className="modal">
        <div className="modal-header">
          <h2>Delivery Notes for SO #{salesOrderId}</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <div className="data-table-container">
            <div className="table-header">
              <h2>Delivery Note List</h2>
              {showCreateButton && (
                <button className="add-button" onClick={onCreateNew}>+ Create New Delivery</button>
              )}
            </div>
            <table className="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Delivery Number</th>
                  <th>Status</th>
                  <th>Delivery Date</th>
                </tr>
              </thead>
              <tbody>
                {notes.map((note) => (
                  <tr
                    key={note.id}
                    className="search-row"
                    style={{ cursor: 'pointer' }}
                    onClick={() => onSelect(note.id)}
                  >
                    <td>{note.id}</td>
                    <td>{note.deliveryNumber}</td>
                    <td>
                      <span className={`status ${note.status.toLowerCase()}`}>
                        {note.status}
                      </span>
                    </td>
                    <td>{formatDate(note.deliveryDate)}</td>
                  </tr>
                ))}
                {notes.length === 0 && (
                  <tr>
                    <td colSpan={4} style={{ textAlign: 'center', padding: '20px' }}>
                      No delivery notes found for this sales order
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
