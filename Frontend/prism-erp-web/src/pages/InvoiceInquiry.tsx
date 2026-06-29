import { useState, useEffect, useCallback } from 'react';
import { invoicesApi, paymentsApi } from '../services/financeApi';
import type { InvoiceDto } from '../services/types/finance.types';
import { formatFinanceInvoiceStatus, formatInvoiceType } from '../services/types/finance.types';
import { StatusBadge } from '../utils/statusBadge';
import { LoadingButton } from '../components/LoadingButton';
import { InvoiceViewModal } from '../components/finance/InvoiceViewModal';
import { PaymentFormModal } from '../components/finance/PaymentFormModal';
import { useToast } from '../hooks/useToast';
import { formatCurrency, formatDate } from '../utils/formatters';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import './Inventory.css';
import './Purchasing.css';

export default function InvoiceInquiry() {
  const { toast, showToast } = useToast();
  const [invoices, setInvoices] = useState<InvoiceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [viewInvoice, setViewInvoice] = useState<InvoiceDto | null>(null);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [paymentInvoice, setPaymentInvoice] = useState<InvoiceDto | null>(null);
  const [paymentNumber, setPaymentNumber] = useState('');
  const [openingPaymentId, setOpeningPaymentId] = useState<number | null>(null);

  const loadInvoices = useCallback(async () => {
    const data = await invoicesApi.getAll();
    setInvoices(data);
  }, []);

  useEffect(() => {
    loadInvoices()
      .catch(() => setInvoices([]))
      .finally(() => setLoading(false));
  }, [loadInvoices]);

  const handleView = async (id: number) => {
    try {
      const invoice = await invoicesApi.getById(id);
      setViewInvoice(invoice);
      setShowInvoiceModal(true);
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleCreatePayment = async (invoice: InvoiceDto) => {
    setOpeningPaymentId(invoice.id);
    try {
      const number = await paymentsApi.generateNumber();
      setPaymentNumber(number);
      setPaymentInvoice(invoice);
      setShowPaymentModal(true);
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setOpeningPaymentId(null);
    }
  };

  const handlePaymentCreate = async (request: Parameters<typeof paymentsApi.create>[0]) => {
    try {
      await paymentsApi.create(request);
      showToast('Payment created successfully!', 'success');
      setShowPaymentModal(false);
      await loadInvoices();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>🧾 Invoice Inquiry</h1>
        <p>View all invoices and create payments</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Invoice Number</th>
                <th>Type</th>
                <th>Status</th>
                <th>Counterparty</th>
                <th>Total</th>
                <th>Paid</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((inv) => (
                <tr key={inv.id}>
                  <td>{inv.id}</td>
                  <td>{inv.invoiceNumber}</td>
                  <td>{formatInvoiceType(inv.invoiceType)}</td>
                  <td><StatusBadge status={inv.status} label={formatFinanceInvoiceStatus(inv.status)} /></td>
                  <td>{inv.counterpartyName || 'N/A'}</td>
                  <td>{formatCurrency(inv.totalAmount)}</td>
                  <td>{formatCurrency(inv.paidAmount)}</td>
                  <td>{formatDate(inv.invoiceDate)}</td>
                  <td className="actions-cell">
                    <button type="button" className="action-btn edit" onClick={() => handleView(inv.id)}>View</button>
                    {String(inv.status) !== 'Paid' && inv.status !== 2 && (
                      <LoadingButton
                        variant="approve"
                        onClick={() => handleCreatePayment(inv)}
                        loading={openingPaymentId === inv.id}
                        loadingText="..."
                      >
                        Create Payment
                      </LoadingButton>
                    )}
                  </td>
                </tr>
              ))}
              {invoices.length === 0 && (
                <tr><td colSpan={9} style={{ textAlign: 'center', padding: '20px' }}>No invoices found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {toast && <div className={`toast toast-${toast.type}`}>{toast.message}</div>}

      <InvoiceViewModal
        isOpen={showInvoiceModal}
        invoice={viewInvoice}
        onClose={() => setShowInvoiceModal(false)}
      />

      <PaymentFormModal
        isOpen={showPaymentModal}
        mode="create"
        payment={null}
        invoiceId={paymentInvoice?.id}
        defaultAmount={paymentInvoice ? paymentInvoice.totalAmount - paymentInvoice.paidAmount : 0}
        defaultPaymentNumber={paymentNumber}
        onClose={() => setShowPaymentModal(false)}
        onCreate={handlePaymentCreate}
      />
    </div>
  );
}
