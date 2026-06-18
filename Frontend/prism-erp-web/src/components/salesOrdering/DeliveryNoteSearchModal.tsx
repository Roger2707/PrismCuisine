import type { DeliveryNoteSummaryDto } from '../../services/types/salesOrdering.types';
import { isSoPartialDelivery } from '../../pages/salesOrdering/delivery/statusHelpers';
import { formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';

interface DeliveryNoteSearchModalProps {
  isOpen: boolean;
  salesOrderId?: number;
  soStatus?: string;
  notes: DeliveryNoteSummaryDto[];
  onClose: () => void;
  onSelect: (deliveryId: number) => void;
  onCreateNew: () => void;
}

export function DeliveryNoteSearchModal({
  isOpen,
  salesOrderId,
  soStatus,
  notes,
  onClose,
  onSelect,
  onCreateNew,
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
                    <td><StatusBadge status={note.status} /></td>
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
