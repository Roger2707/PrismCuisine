import type { CustomerDto } from '../../services/types/salesOrdering.types';
import type { ProductDto } from '../../services/types/inventory.types';
import type { OrderDetail, OrderLineEditable } from '../../pages/salesOrdering/types';
import {
  canApproveSalesOrder,
  canCancelSalesOrder,
  canEditSalesOrder,
} from '../../pages/salesOrdering/orderActionHelpers';
import { canOpenDeliveryNote } from '../../pages/salesOrdering/delivery/statusHelpers';
import { LoadingButton } from '../LoadingButton';
import { SalesOrderCreateForm } from './SalesOrderCreateForm';
import { SalesOrderEditForm } from './SalesOrderEditForm';

interface CreateFormProps {
  createCustomer: CustomerDto | null;
  createNotes: string;
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  onCustomerChange: (customer: CustomerDto | null) => void;
  onNotesChange: (notes: string) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

interface EditFormProps {
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  isReadOnly: boolean;
  onOrderDetailChange: (detail: OrderDetail) => void;
  onCustomerChange: (customer: CustomerDto | null) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

interface SalesOrderModalProps {
  showModal: boolean;
  isCreating: boolean;
  loadingDetail: boolean;
  orderDetail: OrderDetail | null;
  createFormProps: CreateFormProps;
  editFormProps: EditFormProps | null;
  submitting?: 'create' | 'save' | 'approve' | 'cancel' | null;
  onClose: () => void;
  onCreateSave: () => void;
  onSave: () => void;
  onApprove: () => void;
  onCancelOrder: () => void;
  onOpenDeliveryNote: () => void;
  isDeliveryNoteDisabled: boolean;
}

export function SalesOrderModal({
  showModal,
  isCreating,
  loadingDetail,
  orderDetail,
  createFormProps,
  editFormProps,
  submitting = null,
  onClose,
  onCreateSave,
  onSave,
  onApprove,
  onCancelOrder,
  onOpenDeliveryNote,
  isDeliveryNoteDisabled,
}: SalesOrderModalProps) {
  if (!showModal) return null;

  const busy = submitting !== null;
  const status = orderDetail?.status ?? '';
  const showCancelOrder = orderDetail ? canCancelSalesOrder(status) : false;
  const showApprove = orderDetail ? canApproveSalesOrder(status) : false;
  const showSave = orderDetail ? canEditSalesOrder(status) : false;
  const showDeliveryNote = orderDetail ? canOpenDeliveryNote(status) : false;

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>{isCreating ? 'Create New Order' : orderDetail ? 'Order Details' : 'Sales Order'}</h2>
          <button className="close-button" onClick={onClose} disabled={busy}>×</button>
        </div>
        <div className="modal-body">
          {loadingDetail ? (
            <div className="loading">Loading order details...</div>
          ) : isCreating ? (
            <SalesOrderCreateForm {...createFormProps} />
          ) : orderDetail && editFormProps ? (
            <SalesOrderEditForm orderDetail={orderDetail} {...editFormProps} />
          ) : null}
        </div>
        {isCreating && (
          <div className="modal-footer">
            <LoadingButton variant="secondary" onClick={onClose} disabled={busy}>Close</LoadingButton>
            <LoadingButton variant="primary" onClick={onCreateSave} loading={submitting === 'create'} loadingText="Creating...">
              Create Order
            </LoadingButton>
          </div>
        )}
        {orderDetail && (
          <div className="modal-footer">
            <LoadingButton variant="secondary" onClick={onClose} disabled={busy}>Close</LoadingButton>
            {showCancelOrder && (
              <LoadingButton variant="danger" onClick={onCancelOrder} loading={submitting === 'cancel'} loadingText="Cancelling..." disabled={busy}>
                Cancel Order
              </LoadingButton>
            )}
            {showDeliveryNote && (
              <LoadingButton
                variant="action"
                onClick={onOpenDeliveryNote}
                disabled={isDeliveryNoteDisabled || busy}
                title={isDeliveryNoteDisabled ? 'Sales order must be confirmed before creating delivery note' : 'Open delivery note'}
              >
                Delivery Note
              </LoadingButton>
            )}
            {showApprove && (
              <LoadingButton
                variant="approve"
                onClick={onApprove}
                loading={submitting === 'approve'}
                loadingText="Approving..."
                disabled={busy}
              >
                Approve
              </LoadingButton>
            )}
            {showSave && (
              <LoadingButton variant="primary" onClick={onSave} loading={submitting === 'save'} loadingText="Saving..." disabled={busy}>
                Save Changes
              </LoadingButton>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
