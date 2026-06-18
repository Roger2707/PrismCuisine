import { useToast } from '../hooks/useToast';
import { usePurchaseOrderList } from './purchasing/hooks/usePurchaseOrderList';
import { usePurchaseOrderForm } from './purchasing/hooks/usePurchaseOrderForm';
import { usePurchaseOrderLines } from './purchasing/hooks/usePurchaseOrderLines';
import { useGoodsReceiptFromPO } from './purchasing/goodsReceipt/useGoodsReceiptFromPO';
import { PurchaseOrderList } from '../components/purchasing/PurchaseOrderList';
import { PurchaseOrderModal } from '../components/purchasing/PurchaseOrderModal';
import { GoodsReceiptEditModal } from '../components/purchasing/GoodsReceiptEditModal';
import { GoodsReceiptSearchModal } from '../components/purchasing/GoodsReceiptSearchModal';
import { InvoiceViewModal } from '../components/finance/InvoiceViewModal';
import './Inventory.css';
import './Purchasing.css';

export default function Purchasing() {
  const { toast, showToast } = useToast();
  const { orders, loading, refreshOrders, handleDelete, handleApproveInline } = usePurchaseOrderList(showToast);

  const form = usePurchaseOrderForm({ showToast, refreshOrders });
  const lines = usePurchaseOrderLines({
    editableLines: form.editableLines,
    setEditableLines: form.setEditableLines,
    orderDetail: form.orderDetail,
    setOrderDetail: form.setOrderDetail,
  });

  const goodsReceipt = useGoodsReceiptFromPO({
    showToast,
    onPurchaseOrderChanged: form.refreshOrderDetail,
  });

  const handleOpenGoodsReceipt = () => {
    if (form.orderDetail) goodsReceipt.openGoodsReceiptFromPo(form.orderDetail);
  };

  if (loading) return <div className="loading">Loading...</div>;

  const isReadOnly = form.orderDetail?.status !== 'Draft';

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>🛒 Purchasing Module</h1>
        <p>Manage purchase orders</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>Purchase Order List</h2>
            <button className="add-button" onClick={form.handleAdd}>+ Create Order</button>
          </div>
          <PurchaseOrderList
            orders={orders}
            onEdit={form.handleEdit}
            onApprove={handleApproveInline}
            onCancel={handleDelete}
          />
        </div>
      </div>

      <PurchaseOrderModal
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
        onOpenGoodsReceipt={handleOpenGoodsReceipt}
        isGoodsReceiptDisabled={form.orderDetail ? goodsReceipt.isGoodsReceiptButtonDisabled(form.orderDetail.status) : true}
        createFormProps={{
          createSupplier: form.createSupplier,
          createNotes: form.createNotes,
          editableLines: form.editableLines,
          fieldErrors: form.fieldErrors,
          onSupplierChange: form.setCreateSupplier,
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
          onSupplierChange: lines.handleSupplierChange,
          onLineChange: lines.handleLineChange,
          onProductChange: lines.handleCreateLineProductChange,
          onAddLine: lines.handleAddLine,
          onRemoveLine: lines.handleRemoveLine,
        } : null}
      />

      {toast && <div className={`toast toast-${toast.type}`}>{toast.message}</div>}

      <GoodsReceiptEditModal
        isOpen={goodsReceipt.showEditModal}
        loading={goodsReceipt.loading}
        goodsReceipt={goodsReceipt.goodsReceipt}
        lines={goodsReceipt.goodsReceiptLines}
        onClose={goodsReceipt.closeEditModal}
        onLineChange={goodsReceipt.handleLineChange}
        onNotesChange={(notes) => {
          if (goodsReceipt.goodsReceipt) {
            goodsReceipt.setGoodsReceipt({ ...goodsReceipt.goodsReceipt, notes });
          }
        }}
        onSave={goodsReceipt.handleSave}
        onPost={goodsReceipt.handlePost}
        onViewInvoice={goodsReceipt.handleViewInvoice}
        onCreateInvoice={goodsReceipt.handleCreateInvoice}
        showViewInvoice={goodsReceipt.isPostedReceipt && goodsReceipt.hasLinkedInvoice}
        showCreateInvoice={goodsReceipt.isPostedReceipt && !goodsReceipt.hasLinkedInvoice}
        creatingInvoice={goodsReceipt.creatingInvoice}
        saving={goodsReceipt.saving}
        posting={goodsReceipt.posting}
        viewingInvoice={goodsReceipt.invoiceLoading}
      />

      <InvoiceViewModal
        isOpen={goodsReceipt.showInvoiceModal}
        invoice={goodsReceipt.linkedInvoice}
        loading={goodsReceipt.invoiceLoading}
        onClose={goodsReceipt.closeInvoiceModal}
      />

      <GoodsReceiptSearchModal
        isOpen={goodsReceipt.showSearchModal}
        purchaseOrderId={goodsReceipt.activePurchaseOrder?.id}
        poStatus={goodsReceipt.activePurchaseOrder?.status}
        receipts={goodsReceipt.goodsReceiptList}
        onClose={goodsReceipt.closeSearchModal}
        onSelect={goodsReceipt.handleSelectGoodsReceipt}
        onCreateNew={goodsReceipt.handleCreateNewGoodsReceipt}
      />
    </div>
  );
}
