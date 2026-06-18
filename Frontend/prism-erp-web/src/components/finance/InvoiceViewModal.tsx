import { formatCurrency, formatDate } from '../../utils/formatters';
import type { InvoiceDto } from '../../services/types/finance.types';
import { formatFinanceInvoiceStatus, formatInvoiceType } from '../../services/types/finance.types';
import { StatusBadge } from '../../utils/statusBadge';

interface InvoiceViewModalProps {
  isOpen: boolean;
  invoice: InvoiceDto | null;
  loading?: boolean;
  onClose: () => void;
}

export function InvoiceViewModal({ isOpen, invoice, loading, onClose }: InvoiceViewModalProps) {
  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>Invoice Details</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          {loading ? (
            <div className="loading">Loading invoice...</div>
          ) : invoice ? (
            <div className="order-detail">
              <div className="order-info-section">
                <h3>Invoice Information</h3>
                <div className="info-grid">
                  <div className="info-item"><label>Invoice Number:</label><span>{invoice.invoiceNumber}</span></div>
                  <div className="info-item"><label>Type:</label><span>{formatInvoiceType(invoice.invoiceType)}</span></div>
                  <div className="info-item">
                    <label>Status:</label>
                    <StatusBadge status={invoice.status} label={formatFinanceInvoiceStatus(invoice.status)} />
                  </div>
                  <div className="info-item"><label>Invoice Date:</label><span>{formatDate(invoice.invoiceDate)}</span></div>
                  <div className="info-item"><label>Due Date:</label><span>{formatDate(invoice.dueDate)}</span></div>
                  <div className="info-item"><label>Counterparty:</label><span>{invoice.counterpartyName || 'N/A'}</span></div>
                  {invoice.salesOrderId && (
                    <div className="info-item"><label>Sales Order ID:</label><span>{invoice.salesOrderId}</span></div>
                  )}
                  {invoice.deliveryNoteId && (
                    <div className="info-item"><label>Delivery Note ID:</label><span>{invoice.deliveryNoteId}</span></div>
                  )}
                  {invoice.purchaseOrderId && (
                    <div className="info-item"><label>Purchase Order ID:</label><span>{invoice.purchaseOrderId}</span></div>
                  )}
                  {invoice.goodsReceiptId && (
                    <div className="info-item"><label>Goods Receipt ID:</label><span>{invoice.goodsReceiptId}</span></div>
                  )}
                </div>
                {invoice.notes && (
                  <div className="form-group">
                    <label>Notes:</label>
                    <p>{invoice.notes}</p>
                  </div>
                )}
              </div>
              <div className="order-lines-section">
                <h3>Invoice Lines</h3>
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>Qty</th>
                      <th>Unit Price</th>
                      <th>Tax</th>
                      <th>Discount</th>
                      <th>Line Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {invoice.lines.map((line) => (
                      <tr key={line.id}>
                        <td>{line.productName || `Product ${line.productId}`}</td>
                        <td>{line.quantity}</td>
                        <td>{formatCurrency(line.unitPrice)}</td>
                        <td>{formatCurrency(line.taxAmount)}</td>
                        <td>{formatCurrency(line.discountAmount)}</td>
                        <td>{formatCurrency(line.lineTotal)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <div className="order-totals-footer">
                <div className="totals-grid">
                  <div className="total-item"><label>Sub Total:</label><span>{formatCurrency(invoice.subTotal)}</span></div>
                  <div className="total-item"><label>Tax:</label><span>{formatCurrency(invoice.taxAmount)}</span></div>
                  <div className="total-item"><label>Discount:</label><span>-{formatCurrency(invoice.discountAmount)}</span></div>
                  <div className="total-item"><label>Paid:</label><span>{formatCurrency(invoice.paidAmount)}</span></div>
                  <div className="total-item total-amount-item">
                    <label>Total Amount:</label>
                    <span className="grand-total">{formatCurrency(invoice.totalAmount)}</span>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="loading">Invoice not found</div>
          )}
        </div>
        <div className="modal-footer">
          <button className="cancel-button" onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}
