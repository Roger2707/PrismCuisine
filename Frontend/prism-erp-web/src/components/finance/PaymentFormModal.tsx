import { useState, useEffect } from 'react';
import type { PaymentDto, PaymentMethod, CreatePaymentRequest, UpdatePaymentRequest } from '../../services/types/finance.types';
import { PAYMENT_METHOD_OPTIONS, formatPaymentMethod, formatPaymentStatus } from '../../services/types/finance.types';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';
import { LoadingButton } from '../LoadingButton';

interface PaymentFormModalProps {
  isOpen: boolean;
  mode: 'create' | 'view';
  payment: PaymentDto | null;
  invoiceId?: number;
  defaultAmount?: number;
  defaultPaymentNumber?: string;
  loading?: boolean;
  onClose: () => void;
  onCreate?: (request: CreatePaymentRequest) => Promise<void>;
  onUpdate?: (id: number, request: UpdatePaymentRequest) => Promise<void>;
  onComplete?: (id: number) => Promise<void>;
  onFail?: (id: number) => Promise<void>;
  onCancel?: (id: number) => Promise<void>;
}

function isPendingStatus(status: PaymentDto['status']): boolean {
  return String(status) === 'Pending' || status === 1;
}

export function PaymentFormModal({
  isOpen,
  mode,
  payment,
  invoiceId,
  defaultAmount = 0,
  defaultPaymentNumber = '',
  loading,
  onClose,
  onCreate,
  onUpdate,
  onComplete,
  onFail,
  onCancel,
}: PaymentFormModalProps) {
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(1);
  const [amount, setAmount] = useState(defaultAmount);
  const [paymentNumber, setPaymentNumber] = useState(defaultPaymentNumber);
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10));
  const [referenceNumber, setReferenceNumber] = useState('');
  const [bankName, setBankName] = useState('');
  const [accountNumber, setAccountNumber] = useState('');
  const [notes, setNotes] = useState('');
  const [saving, setSaving] = useState(false);
  const [completing, setCompleting] = useState(false);
  const [failing, setFailing] = useState(false);
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    if (mode === 'create') {
      setAmount(defaultAmount);
      setPaymentNumber(defaultPaymentNumber);
      setPaymentDate(new Date().toISOString().slice(0, 10));
      setPaymentMethod(1);
      setReferenceNumber('');
      setBankName('');
      setAccountNumber('');
      setNotes('');
    } else if (payment) {
      setPaymentMethod(payment.paymentMethod);
      setReferenceNumber(payment.referenceNumber || '');
      setBankName(payment.bankName || '');
      setAccountNumber(payment.accountNumber || '');
      setNotes(payment.notes || '');
    }
  }, [mode, payment, defaultAmount, defaultPaymentNumber, isOpen]);

  if (!isOpen) return null;

  const isCreate = mode === 'create';
  const isEditable = isCreate || (payment && isPendingStatus(payment.status));
  const anyLoading = saving || completing || failing || cancelling;

  const handleSave = async () => {
    setSaving(true);
    try {
      if (isCreate && onCreate && invoiceId) {
        await onCreate({
          invoiceId,
          paymentNumber,
          paymentMethod,
          amount,
          paymentDate: new Date(paymentDate).toISOString(),
          referenceNumber: referenceNumber || undefined,
          bankName: bankName || undefined,
          accountNumber: accountNumber || undefined,
          notes: notes || undefined,
        });
      } else if (payment && onUpdate) {
        await onUpdate(payment.id, {
          paymentMethod,
          referenceNumber: referenceNumber || undefined,
          bankName: bankName || undefined,
          accountNumber: accountNumber || undefined,
          notes: notes || undefined,
        });
      }
    } finally {
      setSaving(false);
    }
  };

  const runAction = async (setter: (v: boolean) => void, fn?: (id: number) => Promise<void>) => {
    if (!payment || !fn) return;
    setter(true);
    try {
      await fn(payment.id);
    } finally {
      setter(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>{isCreate ? 'Create Payment' : 'Payment Details'}</h2>
          <button className="close-button" onClick={onClose} disabled={anyLoading}>×</button>
        </div>
        <div className="modal-body">
          {loading ? (
            <div className="loading">Loading payment...</div>
          ) : (
            <>
              {!isCreate && payment && (
                <div className="payment-form-card" style={{ marginBottom: '16px' }}>
                  <h4>Summary</h4>
                  <div className="payment-summary-row"><span>Payment Number</span><span>{payment.paymentNumber}</span></div>
                  <div className="payment-summary-row"><span>Status</span><StatusBadge status={payment.status} label={formatPaymentStatus(payment.status)} /></div>
                  <div className="payment-summary-row"><span>Amount</span><span>{formatCurrency(payment.amount)}</span></div>
                  <div className="payment-summary-row"><span>Payment Date</span><span>{formatDate(payment.paymentDate)}</span></div>
                </div>
              )}

              {isCreate && (
                <div className="payment-form-card" style={{ marginBottom: '16px' }}>
                  <h4>Invoice</h4>
                  <div className="payment-summary-row"><span>Invoice ID</span><span>#{invoiceId}</span></div>
                  <div className="payment-summary-row"><span>Amount Due</span><span>{formatCurrency(defaultAmount)}</span></div>
                </div>
              )}

              <div className="payment-form-card">
                <h4>{isCreate ? 'Payment Details' : 'Edit Details'}</h4>
                <div className="payment-form-grid">
                  {isCreate && (
                    <>
                      <div className="payment-field">
                        <label>Payment Number</label>
                        <input type="text" value={paymentNumber} onChange={(e) => setPaymentNumber(e.target.value)} />
                      </div>
                      <div className="payment-field">
                        <label>Amount (VND)</label>
                        <input type="number" value={amount} onChange={(e) => setAmount(parseFloat(e.target.value) || 0)} min="0" step="0.01" />
                      </div>
                      <div className="payment-field">
                        <label>Payment Date</label>
                        <input type="date" value={paymentDate} onChange={(e) => setPaymentDate(e.target.value)} />
                      </div>
                    </>
                  )}
                  <div className="payment-field">
                    <label>Payment Method</label>
                    {isEditable ? (
                      <select value={String(paymentMethod)} onChange={(e) => setPaymentMethod(Number(e.target.value) as PaymentMethod)}>
                        {PAYMENT_METHOD_OPTIONS.map((opt) => (
                          <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                      </select>
                    ) : (
                      <input disabled value={formatPaymentMethod(paymentMethod)} />
                    )}
                  </div>
                  <div className="payment-field">
                    <label>Reference Number</label>
                    <input type="text" value={referenceNumber} onChange={(e) => setReferenceNumber(e.target.value)} disabled={!isEditable} />
                  </div>
                  <div className="payment-field">
                    <label>Bank Name</label>
                    <input type="text" value={bankName} onChange={(e) => setBankName(e.target.value)} disabled={!isEditable} />
                  </div>
                  <div className="payment-field">
                    <label>Account Number</label>
                    <input type="text" value={accountNumber} onChange={(e) => setAccountNumber(e.target.value)} disabled={!isEditable} />
                  </div>
                  <div className="payment-field payment-field-full">
                    <label>Notes</label>
                    <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={3} disabled={!isEditable} />
                  </div>
                </div>
              </div>
            </>
          )}
        </div>
        <div className="modal-footer">
          <LoadingButton variant="secondary" onClick={onClose} disabled={anyLoading}>Close</LoadingButton>
          <div className="modal-footer-actions">
            {isEditable && (
              <LoadingButton variant="primary" onClick={handleSave} loading={saving} loadingText="Saving...">
                {isCreate ? 'Create Payment' : 'Save Changes'}
              </LoadingButton>
            )}
            {!isCreate && payment && isPendingStatus(payment.status) && (
              <>
                {onComplete && (
                  <LoadingButton variant="approve" onClick={() => runAction(setCompleting, onComplete)} loading={completing} loadingText="Completing...">
                    Complete
                  </LoadingButton>
                )}
                {onFail && (
                  <LoadingButton variant="danger" onClick={() => runAction(setFailing, onFail)} loading={failing} loadingText="Processing...">
                    Fail
                  </LoadingButton>
                )}
                {onCancel && (
                  <LoadingButton variant="danger" onClick={() => runAction(setCancelling, onCancel)} loading={cancelling} loadingText="Cancelling...">
                    Cancel Payment
                  </LoadingButton>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
