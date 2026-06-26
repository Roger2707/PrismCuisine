import type { SupplierDto } from '../../services/types/purchasing.types';
import type { ProductDto } from '../../services/types/inventory.types';
import type { OrderDetail, OrderLineEditable } from '../../pages/purchasing/types';
import {
  canApprovePurchaseOrder,
  canCancelPurchaseOrder,
  canEditPurchaseOrder,
} from '../../pages/purchasing/orderActionHelpers';
import { canOpenGoodsReceipt } from '../../pages/purchasing/goodsReceipt/statusHelpers';
import { LoadingButton } from '../LoadingButton';
import { PurchaseOrderCreateForm } from './PurchaseOrderCreateForm';
import { PurchaseOrderEditForm } from './PurchaseOrderEditForm';

interface CreateFormProps {
  createSupplier: SupplierDto | null;
  createNotes: string;
  editableLines: OrderLineEditable[];
  fieldErrors: Record<string, string>;
  onSupplierChange: (supplier: SupplierDto | null) => void;
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
  onSupplierChange: (supplier: SupplierDto | null) => void;
  onLineChange: (index: number, field: keyof OrderLineEditable, value: number) => void;
  onProductChange: (index: number, product: ProductDto | null) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
}

interface PurchaseOrderModalProps {
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
  onOpenGoodsReceipt: () => void;
  isGoodsReceiptDisabled: boolean;
}

export function PurchaseOrderModal({
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
  onOpenGoodsReceipt,
  isGoodsReceiptDisabled,
}: PurchaseOrderModalProps) {
  if (!showModal) return null;

  const busy = submitting !== null;
  const status = orderDetail?.status ?? '';
  const showCancelOrder = orderDetail ? canCancelPurchaseOrder(status) : false;
  const showApprove = orderDetail ? canApprovePurchaseOrder(status) : false;
  const showSave = orderDetail ? canEditPurchaseOrder(status) : false;
  const showGoodsReceipt = orderDetail ? canOpenGoodsReceipt(status) : false;

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>{orderDetail ? 'Order Details' : 'Create New Order'}</h2>
          <button className="close-button" onClick={onClose} disabled={busy}>×</button>
        </div>
        <div className="modal-body">
          {loadingDetail ? (
            <div className="loading">Loading order details...</div>
          ) : orderDetail && editFormProps ? (
            <PurchaseOrderEditForm orderDetail={orderDetail} {...editFormProps} />
          ) : isCreating ? (
            <PurchaseOrderCreateForm {...createFormProps} />
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
            {showGoodsReceipt && (
              <LoadingButton
                variant="action"
                onClick={onOpenGoodsReceipt}
                disabled={isGoodsReceiptDisabled || busy}
                title={isGoodsReceiptDisabled ? 'Purchase order must be approved before creating goods receipt' : 'Open goods receipt'}
              >
                Goods Receipt
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
