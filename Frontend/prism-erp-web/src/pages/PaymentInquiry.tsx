import { useState, useEffect, useCallback } from 'react';
import { paymentsApi } from '../services/financeApi';
import type { PaymentDto, UpdatePaymentRequest } from '../services/types/finance.types';
import { formatPaymentMethod, formatPaymentStatus } from '../services/types/finance.types';
import { StatusBadge } from '../utils/statusBadge';
import { PaymentFormModal } from '../components/finance/PaymentFormModal';
import { useToast } from '../hooks/useToast';
import { formatCurrency, formatDate } from '../utils/formatters';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import './Inventory.css';
import './Purchasing.css';

export default function PaymentInquiry() {
  const { toast, showToast } = useToast();
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedPayment, setSelectedPayment] = useState<PaymentDto | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);

  const loadPayments = useCallback(async () => {
    const data = await paymentsApi.getAll();
    setPayments(data);
  }, []);

  useEffect(() => {
    loadPayments()
      .catch(() => setPayments([]))
      .finally(() => setLoading(false));
  }, [loadPayments]);

  const handleView = async (id: number) => {
    setShowModal(true);
    setDetailLoading(true);
    try {
      const payment = await paymentsApi.getById(id);
      setSelectedPayment(payment);
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
      setShowModal(false);
    } finally {
      setDetailLoading(false);
    }
  };

  const handleUpdate = async (id: number, request: UpdatePaymentRequest) => {
    try {
      await paymentsApi.update(id, request);
      showToast('Payment updated successfully!', 'success');
      const updated = await paymentsApi.getById(id);
      setSelectedPayment(updated);
      await loadPayments();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleComplete = async (id: number) => {
    try {
      await paymentsApi.complete(id);
      showToast('Payment completed!', 'success');
      setShowModal(false);
      await loadPayments();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleFail = async (id: number) => {
    try {
      await paymentsApi.fail(id);
      showToast('Payment marked as failed', 'success');
      setShowModal(false);
      await loadPayments();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleCancel = async (id: number) => {
    try {
      await paymentsApi.cancel(id);
      showToast('Payment cancelled', 'success');
      setShowModal(false);
      await loadPayments();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>💳 Payment Inquiry</h1>
        <p>View and manage payments</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Payment Number</th>
                <th>Invoice ID</th>
                <th>Method</th>
                <th>Status</th>
                <th>Amount</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {payments.map((p) => (
                <tr key={p.id}>
                  <td>{p.id}</td>
                  <td>{p.paymentNumber}</td>
                  <td>{p.invoiceId}</td>
                  <td>{formatPaymentMethod(p.paymentMethod)}</td>
                  <td><StatusBadge status={p.status} label={formatPaymentStatus(p.status)} /></td>
                  <td>{formatCurrency(p.amount)}</td>
                  <td>{formatDate(p.paymentDate)}</td>
                  <td>
                    <button className="action-btn edit" onClick={() => handleView(p.id)}>View / Edit</button>
                  </td>
                </tr>
              ))}
              {payments.length === 0 && (
                <tr><td colSpan={8} style={{ textAlign: 'center', padding: '20px' }}>No payments found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {toast && <div className={`toast toast-${toast.type}`}>{toast.message}</div>}

      <PaymentFormModal
        isOpen={showModal}
        mode="view"
        payment={selectedPayment}
        loading={detailLoading}
        onClose={() => setShowModal(false)}
        onUpdate={handleUpdate}
        onComplete={handleComplete}
        onFail={handleFail}
        onCancel={handleCancel}
      />
    </div>
  );
}
