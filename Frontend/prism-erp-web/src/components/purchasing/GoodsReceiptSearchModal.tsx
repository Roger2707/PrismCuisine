import type { GoodsReceiptSummaryDto } from '../../services/types/purchasing.types';
import { isPoPartiallyReceived } from '../../pages/purchasing/goodsReceipt/statusHelpers';
import { formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';

interface GoodsReceiptSearchModalProps {
  isOpen: boolean;
  purchaseOrderId?: number;
  poStatus?: string;
  receipts: GoodsReceiptSummaryDto[];
  onClose: () => void;
  onSelect: (receiptId: number) => void;
  onCreateNew: () => void;
}

export function GoodsReceiptSearchModal({
  isOpen,
  purchaseOrderId,
  poStatus,
  receipts,
  onClose,
  onSelect,
  onCreateNew,
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
                    <td><StatusBadge status={receipt.status} /></td>
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
