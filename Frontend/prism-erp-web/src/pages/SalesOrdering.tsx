import { useToast } from '../hooks/useToast';
import { useSalesOrderList } from './salesOrdering/hooks/useSalesOrderList';
import { useSalesOrderForm } from './salesOrdering/hooks/useSalesOrderForm';
import { useSalesOrderLines } from './salesOrdering/hooks/useSalesOrderLines';
import { useDeliveryNoteFromSO } from './salesOrdering/delivery/useDeliveryNoteFromSO';
import { SalesOrderList } from '../components/salesOrdering/SalesOrderList';
import { SalesOrderModal } from '../components/salesOrdering/SalesOrderModal';
import { DeliveryNoteEditModal } from '../components/salesOrdering/DeliveryNoteEditModal';
import { DeliveryNoteSearchModal } from '../components/salesOrdering/DeliveryNoteSearchModal';
import { InvoiceViewModal } from '../components/finance/InvoiceViewModal';
import './Inventory.css';
import './SalesOrdering.css';
import './Purchasing.css';

export default function SalesOrdering() {
  const { toast, showToast } = useToast();
  const { orders, loading, refreshOrders, handleDelete, handleApproveInline } = useSalesOrderList(showToast);

  const form = useSalesOrderForm({ showToast, refreshOrders });
  const lines = useSalesOrderLines({
    editableLines: form.editableLines,
    setEditableLines: form.setEditableLines,
    orderDetail: form.orderDetail,
    setOrderDetail: form.setOrderDetail,
  });

  const deliveryNote = useDeliveryNoteFromSO({
    showToast,
    onSalesOrderChanged: form.refreshOrderDetail,
  });

  const handleOpenDeliveryNote = () => {
    if (form.orderDetail) deliveryNote.openDeliveryNoteFromSo(form.orderDetail);
  };

  if (loading) return <div className="loading">Loading...</div>;

  const isReadOnly = form.orderDetail?.status !== 'Draft';

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>📋 Sales Order</h1>
        <p>Manage sales orders and deliveries</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>Sales Order List</h2>
            <button className="add-button" onClick={form.handleAdd}>+ Create Order</button>
          </div>
          <SalesOrderList
            orders={orders}
            onEdit={form.handleEdit}
            onApprove={handleApproveInline}
            onCancel={handleDelete}
          />
        </div>
      </div>

      <SalesOrderModal
        showModal={form.showModal}
        isCreating={form.isCreating}
        loadingDetail={form.loadingDetail}
        orderDetail={form.orderDetail}
        onClose={form.handleCancel}
        onCreateSave={form.handleCreateSave}
        onSave={form.handleSave}
        onApprove={form.handleApprove}
        onCancelOrder={form.handleCancelOrder}
        submitting={form.submitting}
        onOpenDeliveryNote={handleOpenDeliveryNote}
        isDeliveryNoteDisabled={form.orderDetail ? deliveryNote.isDeliveryNoteButtonDisabled(form.orderDetail.status) : true}
        createFormProps={{
          createCustomer: form.createCustomer,
          createNotes: form.createNotes,
          editableLines: form.editableLines,
          fieldErrors: form.fieldErrors,
          onCustomerChange: form.setCreateCustomer,
          onNotesChange: form.setCreateNotes,
          onLineChange: lines.handleLineChange,
          onProductChange: lines.handleCreateLineProductChange,
          onAddLine: lines.handleAddLine,
          onRemoveLine: lines.handleRemoveLine,
        }}
        editFormProps={form.orderDetail ? {
          editableLines: form.editableLines,
          fieldErrors: form.fieldErrors,
          isReadOnly,
          onOrderDetailChange: form.setOrderDetail,
          onCustomerChange: lines.handleCustomerChange,
          onLineChange: lines.handleLineChange,
          onProductChange: lines.handleCreateLineProductChange,
          onAddLine: lines.handleAddLine,
          onRemoveLine: lines.handleRemoveLine,
        } : null}
      />

      {toast && <div className={`toast toast-${toast.type}`}>{toast.message}</div>}

      <DeliveryNoteEditModal
        isOpen={deliveryNote.showEditModal}
        loading={deliveryNote.loading}
        deliveryNote={deliveryNote.deliveryNote}
        lines={deliveryNote.deliveryNoteLines}
        onClose={deliveryNote.closeEditModal}
        onLineChange={deliveryNote.handleLineChange}
        onNotesChange={(notes) => {
          if (deliveryNote.deliveryNote) {
            deliveryNote.setDeliveryNote({ ...deliveryNote.deliveryNote, notes });
          }
        }}
        onSave={deliveryNote.handleSave}
        onPost={deliveryNote.handlePost}
        onCancelDelivery={deliveryNote.handleCancelDelivery}
        onViewInvoice={deliveryNote.handleViewInvoice}
        showViewInvoice={deliveryNote.canViewInvoice && deliveryNote.deliveryNote?.status === 'Posted'}
        showCancelDelivery={deliveryNote.canCancelDelivery}
        saving={deliveryNote.saving}
        posting={deliveryNote.posting}
        cancelling={deliveryNote.cancelling}
        viewingInvoice={deliveryNote.invoiceLoading}
      />

      <InvoiceViewModal
        isOpen={deliveryNote.showInvoiceModal}
        invoice={deliveryNote.linkedInvoice}
        loading={deliveryNote.invoiceLoading}
        onClose={deliveryNote.closeInvoiceModal}
      />

      <DeliveryNoteSearchModal
        isOpen={deliveryNote.showSearchModal}
        salesOrderId={deliveryNote.activeSalesOrder?.id}
        soStatus={deliveryNote.activeSalesOrder?.status}
        notes={deliveryNote.deliveryNoteList}
        onClose={deliveryNote.closeSearchModal}
        onSelect={deliveryNote.handleSelectDeliveryNote}
        onCreateNew={deliveryNote.handleCreateNewDeliveryNote}
      />
    </div>
  );
}
