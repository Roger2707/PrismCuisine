import { useState, useEffect, useCallback } from 'react';
import { customersApi, salesOrdersApi } from '../services/salesOrderingApi';
import type { SalesOrderSummaryDto, SalesOrderDto, CustomerDto } from '../services/types/salesOrdering.types';
import type { ProductDto } from '../services/types/inventory.types';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import CustomerSearch from '../components/CustomerSearch';
import ProductSearch from '../components/ProductSearch';
import { DeliveryNoteEditModal, DeliveryNoteSearchModal } from '../components/salesOrdering/DeliveryNoteModals';
import { useDeliveryNoteFromSO } from './salesOrdering/deliveryNoteFromSO';
import './Inventory.css';
import './SalesOrdering.css';
import './Purchasing.css';

interface SalesOrder extends SalesOrderSummaryDto {}

interface OrderDetail extends SalesOrderDto {
  customerData?: CustomerDto;
}

interface OrderLineEditable {
  id: number;
  productId: number;
  productName: string;
  productData?: ProductDto | null;
  quantityOrdered: number;
  quantityDelivered: number;
  quantityRemaining: number;
  unitPrice: number;
  discountPercent: number;
  vatRate: number;
  discountAmount: number;
  vatAmount: number;
  lineTotal: number;
}

export default function SalesOrdering() {
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [orderDetail, setOrderDetail] = useState<OrderDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const [editableLines, setEditableLines] = useState<OrderLineEditable[]>([]);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isCreating, setIsCreating] = useState(false);
  const [createCustomer, setCreateCustomer] = useState<CustomerDto | null>(null);
  const [createNotes, setCreateNotes] = useState('');

  const showToast = useCallback((message: string, type: 'success' | 'error') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), type === 'error' ? 5000 : 3000);
  }, []);

  const refreshOrders = useCallback(async () => {
    const data = await salesOrdersApi.getAll();
    setOrders(data.map((dto) => ({ ...dto })));
  }, []);

  const refreshOrderDetail = useCallback(async () => {
    await refreshOrders();
    if (!orderDetail) return;
    const data = await salesOrdersApi.getById(orderDetail.id);
    const customerData = await customersApi.getById(data.customerId);
    const lines = data.lines.map((line) => ({ ...line }));
    setEditableLines(lines);
    setOrderDetail({ ...data, customerData });
  }, [orderDetail, refreshOrders]);

  const deliveryNote = useDeliveryNoteFromSO({
    showToast,
    onSalesOrderChanged: refreshOrderDetail,
  });

  useEffect(() => {
    refreshOrders()
      .catch(() => {
        setOrders([
          { id: 1, orderNumber: 'SO-2024-001', totalAmount: 2500000, status: 'Draft', orderDate: '2024-01-15', customerId: 1, customerName: 'Sen Vang Restaurant', subTotal: 0, totalDiscount: 0, totalVAT: 0 },
        ]);
      })
      .finally(() => setLoading(false));
  }, [refreshOrders]);

  const calculateLineTotals = (lines: OrderLineEditable[]): OrderLineEditable[] => {
    return lines.map((line) => {
      const gross = line.unitPrice * line.quantityOrdered;
      const discountAmount = gross * (line.discountPercent / 100);
      const afterDiscount = gross - discountAmount;
      const vatAmount = afterDiscount * (line.vatRate / 100);
      return {
        ...line,
        discountAmount,
        vatAmount,
        lineTotal: afterDiscount + vatAmount,
      };
    });
  };

  const calculateOrderTotals = (lines: OrderLineEditable[]) => {
    const subTotal = lines.reduce((sum, line) => sum + line.unitPrice * line.quantityOrdered, 0);
    const totalDiscount = lines.reduce((sum, line) => sum + line.discountAmount, 0);
    const totalVAT = lines.reduce((sum, line) => sum + line.vatAmount, 0);
    const totalAmount = lines.reduce((sum, line) => sum + line.lineTotal, 0);
    return { subTotal, totalDiscount, totalVAT, totalAmount };
  };

  const handleAdd = () => {
    setIsCreating(true);
    setOrderDetail(null);
    setCreateCustomer(null);
    setCreateNotes('');
    setEditableLines([{
      id: Date.now(),
      productId: 0,
      productName: '',
      productData: null,
      quantityOrdered: 0,
      quantityDelivered: 0,
      quantityRemaining: 0,
      unitPrice: 0,
      discountPercent: 0,
      vatRate: 10,
      discountAmount: 0,
      vatAmount: 0,
      lineTotal: 0,
    }]);
    setShowModal(true);
  };

  const handleEdit = async (order: SalesOrder) => {
    setIsCreating(false);
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const data = await salesOrdersApi.getById(order.id);
      const customerData = await customersApi.getById(order.customerId);
      setEditableLines(data.lines.map((line) => ({ ...line })));
      setOrderDetail({ ...data, customerData });
    } catch {
      setEditableLines([]);
      setOrderDetail(null);
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleApprove = async () => {
    if (!orderDetail) return;
    try {
      await handleSave();
      await salesOrdersApi.approve(orderDetail.id);
      showToast('Order approved successfully!', 'success');
      setShowModal(false);
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleCancelOrder = async () => {
    if (!orderDetail) return;
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await salesOrdersApi.cancel(orderDetail.id);
        showToast('Order cancelled successfully!', 'success');
        setShowModal(false);
        await refreshOrders();
      } catch (error: unknown) {
        showToast(getToastMessage(parseApiError(error)), 'error');
      }
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await salesOrdersApi.cancel(id);
        showToast('Order cancelled successfully!', 'success');
        await refreshOrders();
      } catch (error: unknown) {
        showToast(getToastMessage(parseApiError(error)), 'error');
      }
    }
  };

  const handleApproveInline = async (id: number) => {
    try {
      await salesOrdersApi.approve(id);
      showToast('Order approved successfully!', 'success');
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleOpenDeliveryNote = () => {
    if (!orderDetail) return;
    deliveryNote.openDeliveryNoteFromSo(orderDetail);
  };

  const handleLineChange = (index: number, field: keyof OrderLineEditable, value: number) => {
    const updatedLines = [...editableLines];
    updatedLines[index] = { ...updatedLines[index], [field]: value };
    const recalculatedLines = calculateLineTotals(updatedLines);
    setEditableLines(recalculatedLines);
    if (orderDetail) {
      const totals = calculateOrderTotals(recalculatedLines);
      setOrderDetail({ ...orderDetail, ...totals, lines: recalculatedLines });
    }
  };

  const handleCreateLineProductChange = (index: number, product: ProductDto | null) => {
    const updatedLines = [...editableLines];
    updatedLines[index] = {
      ...updatedLines[index],
      productId: product?.id ?? 0,
      productName: product?.name ?? '',
      productData: product,
    };
    setEditableLines(calculateLineTotals(updatedLines));
  };

  const handleAddLine = () => {
    const newLine: OrderLineEditable = {
      id: Date.now(),
      productId: 0,
      productName: '',
      productData: null,
      quantityOrdered: 0,
      quantityDelivered: 0,
      quantityRemaining: 0,
      unitPrice: 0,
      discountPercent: 0,
      vatRate: 10,
      discountAmount: 0,
      vatAmount: 0,
      lineTotal: 0,
    };
    setEditableLines(calculateLineTotals([...editableLines, newLine]));
  };

  const handleRemoveLine = (index: number) => {
    setEditableLines(calculateLineTotals(editableLines.filter((_, i) => i !== index)));
  };

  const handleCreateSave = async () => {
    if (!createCustomer) {
      setFieldErrors({ customerId: 'Customer is required' });
      showToast('Please select a customer', 'error');
      return;
    }
    const validLines = editableLines.filter((line) => line.productId > 0 && line.quantityOrdered > 0);
    if (validLines.length === 0) {
      showToast('Add at least one product line', 'error');
      return;
    }
    try {
      await salesOrdersApi.create({
        customerId: createCustomer.id,
        customerName: createCustomer.name,
        notes: createNotes || undefined,
        lines: validLines.map((line) => ({
          productId: line.productId,
          productName: line.productName,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
          discountPercent: line.discountPercent,
          vatRate: line.vatRate,
        })),
      });
      showToast('Order created successfully!', 'success');
      setShowModal(false);
      setIsCreating(false);
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleSave = async () => {
    if (!orderDetail) return;
    setFieldErrors({});
    try {
      await salesOrdersApi.update(orderDetail.id, {
        customerId: orderDetail.customerId,
        customerName: orderDetail.customerName,
        notes: orderDetail.notes,
        lines: editableLines.map((line) => ({
          productId: line.productId,
          productName: line.productName,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
          discountPercent: line.discountPercent,
          vatRate: line.vatRate,
        })),
      });
      showToast('Order updated successfully!', 'success');
      setShowModal(false);
      await refreshOrders();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      if (apiError.type === 'validation-error' && apiError.fieldErrors) {
        const errors: Record<string, string> = {};
        apiError.fieldErrors.forEach((fe) => {
          if (fe.field.toLowerCase().includes('customer')) errors.customerId = fe.messages[0];
          else errors[fe.field] = fe.messages[0];
        });
        setFieldErrors(errors);
      }
      showToast(getToastMessage(apiError), 'error');
    }
  };

  const handleCancel = () => {
    setShowModal(false);
    setOrderDetail(null);
    setEditableLines([]);
    setFieldErrors({});
    setIsCreating(false);
  };

  const handleCustomerChange = (customer: CustomerDto | null) => {
    if (customer && orderDetail) {
      setOrderDetail({ ...orderDetail, customerId: customer.id, customerName: customer.name, customerData: customer });
    }
    if (!customer && orderDetail) {
      setOrderDetail({ ...orderDetail, customerId: 0, customerName: '', customerData: undefined });
    }
  };

  const isReadOnly = orderDetail?.status !== 'Draft';

  const formatCurrency = (value: number): string =>
    new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(value);

  const formatDate = (date: string | Date | null | undefined): string => {
    if (!date) return 'N/A';
    return new Intl.DateTimeFormat('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date(date));
  };

  const renderLineInputs = (readOnly: boolean, useProductSearch: boolean) => (
    editableLines.map((line, index) => (
      <tr key={line.id}>
        <td>
          {useProductSearch ? (
            <ProductSearch
              value={line.productData ?? null}
              onChange={(product) => handleCreateLineProductChange(index, product)}
            />
          ) : (
            line.productName
          )}
        </td>
        <td>
          <input
            type="number"
            value={line.quantityOrdered}
            onChange={(e) => handleLineChange(index, 'quantityOrdered', parseFloat(e.target.value) || 0)}
            disabled={readOnly}
            min="0"
            step="0.01"
            className="table-input"
          />
        </td>
        {!useProductSearch && (
          <>
            <td>{line.quantityDelivered}</td>
            <td>{line.quantityRemaining}</td>
          </>
        )}
        <td>
          <input
            type="number"
            value={line.unitPrice}
            onChange={(e) => handleLineChange(index, 'unitPrice', parseFloat(e.target.value) || 0)}
            disabled={readOnly}
            min="0"
            step="0.01"
            className="table-input"
          />
        </td>
        <td>
          <input
            type="number"
            value={line.discountPercent}
            onChange={(e) => handleLineChange(index, 'discountPercent', parseFloat(e.target.value) || 0)}
            disabled={readOnly}
            min="0"
            max="100"
            step="0.01"
            className="table-input"
          />
        </td>
        <td>
          <input
            type="number"
            value={line.vatRate}
            onChange={(e) => handleLineChange(index, 'vatRate', parseFloat(e.target.value) || 0)}
            disabled={readOnly}
            min="0"
            max="100"
            step="0.01"
            className="table-input"
          />
        </td>
        <td className="line-total">{formatCurrency(line.lineTotal)}</td>
        {!readOnly && (
          <td>
            <button className="action-btn delete" onClick={() => handleRemoveLine(index)} style={{ padding: '4px 8px', fontSize: '12px' }}>
              Remove
            </button>
          </td>
        )}
      </tr>
    ))
  );

  if (loading) return <div className="loading">Loading...</div>;

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
            <button className="add-button" onClick={handleAdd}>+ Create Order</button>
          </div>
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Order Number</th>
                <th>Customer</th>
                <th>Total Amount</th>
                <th>Status</th>
                <th>Order Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td>{order.id}</td>
                  <td>{order.orderNumber}</td>
                  <td>{order.customerName}</td>
                  <td>{formatCurrency(order.totalAmount)}</td>
                  <td><span className={`status ${order.status.toLowerCase()}`}>{order.status}</span></td>
                  <td>{formatDate(order.orderDate)}</td>
                  <td>
                    <button className="action-btn edit" onClick={() => handleEdit(order)}>Edit</button>
                    <button
                      className="action-btn approve"
                      onClick={() => handleApproveInline(order.id)}
                      disabled={order.status !== 'Draft'}
                    >
                      Approve
                    </button>
                    <button className="action-btn delete" onClick={() => handleDelete(order.id)}>Cancel</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-overlay">
          <div className="modal modal-large">
            <div className="modal-header">
              <h2>{isCreating ? 'Create New Order' : orderDetail ? 'Order Details' : 'Sales Order'}</h2>
              <button className="close-button" onClick={handleCancel}>×</button>
            </div>
            <div className="modal-body">
              {loadingDetail ? (
                <div className="loading">Loading order details...</div>
              ) : isCreating ? (
                <div className="order-detail">
                  <div className="order-info-section">
                    <h3>New Sales Order</h3>
                    <div className="info-grid">
                      <div className="info-item">
                        <label>Customer:</label>
                        <CustomerSearch value={createCustomer} onChange={setCreateCustomer} hasError={!!fieldErrors.customerId} />
                      </div>
                    </div>
                    <div className="form-group">
                      <label>Notes:</label>
                      <textarea value={createNotes} onChange={(e) => setCreateNotes(e.target.value)} rows={3} />
                    </div>
                  </div>
                  <div className="order-lines-section">
                    <h3>Order Lines</h3>
                    <table className="data-table editable-table">
                      <thead>
                        <tr>
                          <th>Product</th>
                          <th>Qty Ordered</th>
                          <th>Unit Price</th>
                          <th>Discount %</th>
                          <th>VAT %</th>
                          <th>Line Total</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>{renderLineInputs(false, true)}</tbody>
                    </table>
                    <button className="add-button" onClick={handleAddLine} style={{ marginTop: '10px' }}>+ Add Line</button>
                  </div>
                </div>
              ) : orderDetail ? (
                <div className="order-detail">
                  <div className="order-info-section">
                    <h3>Order Information</h3>
                    <div className="info-grid">
                      <div className="info-item"><label>Order Number:</label><span>{orderDetail.orderNumber}</span></div>
                      <div className="info-item">
                        <label>Customer:</label>
                        <CustomerSearch value={orderDetail.customerData || null} onChange={handleCustomerChange} disabled={isReadOnly} hasError={!!fieldErrors.customerId} />
                      </div>
                      <div className="info-item"><label>Order Date:</label><span>{formatDate(orderDetail.orderDate)}</span></div>
                      <div className="info-item"><label>Status:</label><span className={`status ${orderDetail.status.toLowerCase()}`}>{orderDetail.status}</span></div>
                    </div>
                    <div className="form-group">
                      <label>Notes:</label>
                      <textarea value={orderDetail.notes || ''} onChange={(e) => setOrderDetail({ ...orderDetail, notes: e.target.value })} rows={3} disabled={isReadOnly} />
                    </div>
                  </div>
                  <div className="order-lines-section">
                    <h3>Order Lines</h3>
                    <table className="data-table editable-table">
                      <thead>
                        <tr>
                          <th>Product</th>
                          <th>Qty Ordered</th>
                          <th>Qty Delivered</th>
                          <th>Qty Remaining</th>
                          <th>Unit Price</th>
                          <th>Discount %</th>
                          <th>VAT %</th>
                          <th>Line Total</th>
                          {!isReadOnly && <th>Actions</th>}
                        </tr>
                      </thead>
                      <tbody>{renderLineInputs(isReadOnly, false)}</tbody>
                    </table>
                    {!isReadOnly && <button className="add-button" onClick={handleAddLine} style={{ marginTop: '10px' }}>+ Add Line</button>}
                  </div>
                  <div className="order-totals-footer">
                    <div className="totals-grid">
                      <div className="total-item"><label>Sub Total:</label><span>{formatCurrency(orderDetail.subTotal)}</span></div>
                      <div className="total-item"><label>Total Discount:</label><span className="discount-value">-{formatCurrency(orderDetail.totalDiscount)}</span></div>
                      <div className="total-item"><label>Total VAT:</label><span>{formatCurrency(orderDetail.totalVAT)}</span></div>
                      <div className="total-item total-amount-item"><label>Total Amount:</label><span className="grand-total">{formatCurrency(orderDetail.totalAmount)}</span></div>
                    </div>
                  </div>
                </div>
              ) : null}
            </div>
            {isCreating && (
              <div className="modal-footer">
                <button className="cancel-button" onClick={handleCancel}>Cancel</button>
                <button className="save-button" onClick={handleCreateSave}>Create Order</button>
              </div>
            )}
            {orderDetail && (
              <div className="modal-footer">
                <button className="cancel-button" onClick={handleCancelOrder}>Cancel Order</button>
                <button
                  className="action-btn goods-receipt"
                  onClick={handleOpenDeliveryNote}
                  disabled={deliveryNote.isDeliveryNoteButtonDisabled(orderDetail.status)}
                  title={
                    deliveryNote.isDeliveryNoteButtonDisabled(orderDetail.status)
                      ? 'Sales order must be confirmed before creating delivery note'
                      : 'Open delivery note'
                  }
                >
                  Delivery Note
                </button>
                <button className="approve-button" onClick={handleApprove} disabled={orderDetail.status !== 'Draft'}>Approve</button>
                <button className="save-button" onClick={handleSave}>Save Changes</button>
              </div>
            )}
          </div>
        </div>
      )}

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
        formatDate={formatDate}
      />

      <DeliveryNoteSearchModal
        isOpen={deliveryNote.showSearchModal}
        salesOrderId={deliveryNote.activeSalesOrder?.id}
        soStatus={deliveryNote.activeSalesOrder?.status}
        notes={deliveryNote.deliveryNoteList}
        onClose={deliveryNote.closeSearchModal}
        onSelect={deliveryNote.handleSelectDeliveryNote}
        onCreateNew={deliveryNote.handleCreateNewDeliveryNote}
        formatDate={formatDate}
      />
    </div>
  );
}
