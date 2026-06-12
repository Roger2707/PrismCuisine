import { useState, useEffect, useCallback } from 'react';
import { suppliersApi, purchaseOrdersApi } from '../services/purchasingApi';
import { warehousesApi } from '../services/inventoryApi';
import type { PurchaseOrderSummaryDto, PurchaseOrderDto, SupplierDto } from '../services/types/purchasing.types';
import type { ProductDto } from '../services/types/inventory.types';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import SupplierSearch from '../components/SupplierSearch';
import ProductSearch from '../components/ProductSearch';
import { GoodsReceiptEditModal, GoodsReceiptSearchModal } from '../components/purchasing/GoodsReceiptModals';
import { useGoodsReceiptFromPO } from './purchasing/goodsReceiptFromPO';
import './Inventory.css';
import './Purchasing.css';

interface PurchaseOrder extends PurchaseOrderSummaryDto {}

interface OrderDetail extends PurchaseOrderDto {
  supplierData?: SupplierDto;
  supplierName?: string;
  totalAmount?: number;
}

interface OrderLineEditable {
  id: number;
  productId: number;
  productName: string;
  productData?: ProductDto | null;
  quantityOrdered: number;
  quantityReceived: number;
  quantityRemaining: number;
  unitPrice: number;
  lineTotal: number;
}

export default function Purchasing() {
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [orderDetail, setOrderDetail] = useState<OrderDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const [editableLines, setEditableLines] = useState<OrderLineEditable[]>([]);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isCreating, setIsCreating] = useState(false);
  const [defaultWarehouseId, setDefaultWarehouseId] = useState(1);
  const [createSupplier, setCreateSupplier] = useState<SupplierDto | null>(null);
  const [createNotes, setCreateNotes] = useState('');

  const showToast = useCallback((message: string, type: 'success' | 'error') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), type === 'error' ? 5000 : 3000);
  }, []);

  const refreshOrders = useCallback(async () => {
    const data = await purchaseOrdersApi.getAll();
    setOrders(data.map((dto) => ({ ...dto })));
  }, []);

  const refreshOrderDetail = useCallback(async () => {
    await refreshOrders();
    if (!orderDetail) return;
    const data = await purchaseOrdersApi.getById(orderDetail.id);
    const supplierData = await suppliersApi.getById(data.supplierId);
    const lines: OrderLineEditable[] = data.lines.map((line) => ({
      ...line,
      productName: `Product ${line.productId}`,
      lineTotal: line.quantityOrdered * line.unitPrice,
    }));
    setEditableLines(lines);
    setOrderDetail({
      ...data,
      supplierData,
      supplierName: supplierData.name,
      totalAmount: lines.reduce((sum, line) => sum + line.lineTotal, 0),
    });
  }, [orderDetail, refreshOrders]);

  const goodsReceipt = useGoodsReceiptFromPO({
    showToast,
    onPurchaseOrderChanged: refreshOrderDetail,
  });

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        await refreshOrders();
      } catch (error) {
        console.log('API not available, using mock data');
        setOrders([]);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, [refreshOrders]);

  useEffect(() => {
    warehousesApi.getAll()
      .then((warehouses) => {
        if (warehouses.length > 0) setDefaultWarehouseId(warehouses[0].id);
      })
      .catch(() => setDefaultWarehouseId(1));
  }, []);

  const handleAdd = () => {
    setIsCreating(true);
    setOrderDetail(null);
    setCreateSupplier(null);
    setCreateNotes('');
    setEditableLines([{
      id: Date.now(),
      productId: 0,
      productName: '',
      productData: null,
      quantityOrdered: 0,
      quantityReceived: 0,
      quantityRemaining: 0,
      unitPrice: 0,
      lineTotal: 0,
    }]);
    setShowModal(true);
  };

  const handleEdit = async (order: PurchaseOrder) => {
    setIsCreating(false);
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const data = await purchaseOrdersApi.getById(order.id);
      const supplierData = await suppliersApi.getById(order.supplierId);
      const lines: OrderLineEditable[] = data.lines.map((line) => ({
        ...line,
        productName: `Product ${line.productId}`,
        lineTotal: line.quantityOrdered * line.unitPrice,
      }));
      setEditableLines(lines);
      setOrderDetail({
        ...data,
        supplierData,
        supplierName: supplierData.name,
        totalAmount: lines.reduce((sum, l) => sum + l.lineTotal, 0),
      });
    } catch (error) {
      console.log('API not available, using mock detail data');
      const mockLines: OrderLineEditable[] = [
        {
          id: 1,
          productId: 1,
          productName: 'Product A',
          quantityOrdered: 10,
          quantityReceived: 0,
          quantityRemaining: 10,
          unitPrice: 100000,
          lineTotal: 1000000,
        },
        {
          id: 2,
          productId: 2,
          productName: 'Product B',
          quantityOrdered: 5,
          quantityReceived: 0,
          quantityRemaining: 5,
          unitPrice: 200000,
          lineTotal: 1000000,
        },
      ];
      setEditableLines(mockLines);
      setOrderDetail({
        id: order.id,
        orderNumber: order.orderNumber,
        supplierId: order.supplierId,
        warehouseId: order.warehouseId,
        status: order.status,
        approvedAt: order.approvedAt,
        amendedFromPurchaseOrderId: order.amendedFromPurchaseOrderId,
        notes: 'Sample notes for the order',
        lines: mockLines,
        supplierName: 'Supplier Name',
      });
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleApprove = async () => {
    if (!orderDetail) return;
    try {
      await handleSave();
      await purchaseOrdersApi.approve(orderDetail.id);
      showToast('Order approved successfully!', 'success');
      setShowModal(false);
      await refreshOrders();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      showToast(getToastMessage(apiError), 'error');
    }
  };

  const handleCancelOrder = async () => {
    if (!orderDetail) return;
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await purchaseOrdersApi.cancel(orderDetail.id);
        showToast('Order cancelled successfully!', 'success');
        setShowModal(false);
        await refreshOrders();
      } catch (error: unknown) {
        const apiError = parseApiError(error);
        showToast(getToastMessage(apiError), 'error');
      }
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await purchaseOrdersApi.cancel(id);
        showToast('Order cancelled successfully!', 'success');
        setOrders(orders.filter((order) => order.id !== id));
      } catch (error: unknown) {
        const apiError = parseApiError(error);
        showToast(getToastMessage(apiError), 'error');
      }
    }
  };

  const handleApproveInline = async (id: number) => {
    try {
      await purchaseOrdersApi.approve(id);
      showToast('Order approved successfully!', 'success');
      await refreshOrders();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      showToast(getToastMessage(apiError), 'error');
    }
  };

  const handleOpenGoodsReceipt = () => {
    if (!orderDetail) return;
    goodsReceipt.openGoodsReceiptFromPo(orderDetail);
  };

  const calculateLineTotals = (lines: OrderLineEditable[]): OrderLineEditable[] => {
    return lines.map((line) => ({
      ...line,
      lineTotal: line.quantityOrdered * line.unitPrice,
    }));
  };

  const calculateOrderTotals = (lines: OrderLineEditable[]) => {
    const totalAmount = lines.reduce((sum, line) => sum + line.lineTotal, 0);
    return { totalAmount };
  };

  const handleLineChange = (index: number, field: keyof OrderLineEditable, value: number) => {
    const updatedLines = [...editableLines];
    updatedLines[index] = { ...updatedLines[index], [field]: value };
    const recalculatedLines = calculateLineTotals(updatedLines);
    setEditableLines(recalculatedLines);

    const totals = calculateOrderTotals(recalculatedLines);
    if (orderDetail) {
      setOrderDetail({
        ...orderDetail,
        totalAmount: totals.totalAmount,
        lines: recalculatedLines,
      });
    }
  };

  const handleAddLine = () => {
    const newLine: OrderLineEditable = {
      id: Date.now(),
      productId: 0,
      productName: '',
      productData: null,
      quantityOrdered: 0,
      quantityReceived: 0,
      quantityRemaining: 0,
      unitPrice: 0,
      lineTotal: 0,
    };
    const updatedLines = [...editableLines, newLine];
    const recalculatedLines = calculateLineTotals(updatedLines);
    setEditableLines(recalculatedLines);

    const totals = calculateOrderTotals(recalculatedLines);
    if (orderDetail) {
      setOrderDetail({
        ...orderDetail,
        totalAmount: totals.totalAmount,
        lines: recalculatedLines,
      });
    }
  };

  const handleRemoveLine = (index: number) => {
    const updatedLines = editableLines.filter((_, i) => i !== index);
    const recalculatedLines = calculateLineTotals(updatedLines);
    setEditableLines(recalculatedLines);

    const totals = calculateOrderTotals(recalculatedLines);
    if (orderDetail) {
      setOrderDetail({
        ...orderDetail,
        totalAmount: totals.totalAmount,
        lines: recalculatedLines,
      });
    }
  };

  const handleCreateSave = async () => {
    setFieldErrors({});
    if (!createSupplier) {
      setFieldErrors({ supplierId: 'Supplier is required' });
      showToast('Please select a supplier', 'error');
      return;
    }

    const validLines = editableLines.filter((line) => line.productId > 0 && line.quantityOrdered > 0);
    if (validLines.length === 0) {
      showToast('Add at least one product line', 'error');
      return;
    }

    try {
      await purchaseOrdersApi.create({
        supplierId: createSupplier.id,
        warehouseId: defaultWarehouseId,
        notes: createNotes || undefined,
        lines: validLines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
        })),
      });
      showToast('Order created successfully!', 'success');
      setShowModal(false);
      setIsCreating(false);
      await refreshOrders();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      showToast(getToastMessage(apiError), 'error');
    }
  };

  const handleSave = async () => {
    if (!orderDetail) return;

    setFieldErrors({});

    try {
      await purchaseOrdersApi.update(orderDetail.id, {
        supplierId: orderDetail.supplierId,
        warehouseId: orderDetail.warehouseId,
        notes: orderDetail.notes,
        lines: editableLines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
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
          const fieldName = fe.field.toLowerCase();
          if (fieldName.includes('supplier')) {
            errors.supplierId = fe.messages[0];
          } else {
            errors[fe.field] = fe.messages[0];
          }
        });
        setFieldErrors(errors);
        setTimeout(() => setFieldErrors({}), 2000);
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

  const handleSupplierChange = (supplier: SupplierDto | null) => {
    if (supplier && orderDetail) {
      setOrderDetail({
        ...orderDetail,
        supplierId: supplier.id,
        supplierName: supplier.name,
        supplierData: supplier,
      });
    }
    if (!supplier && orderDetail) {
      setOrderDetail({
        ...orderDetail,
        supplierId: 0,
        supplierName: '',
        supplierData: undefined,
      });
    }
  };

  const isReadOnly = orderDetail?.status !== 'Draft';

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  };

  const formatDate = (date: string | Date | null | undefined): string => {
    if (!date) return 'N/A';
    return new Intl.DateTimeFormat('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    }).format(new Date(date));
  };

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

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
            <button className="add-button" onClick={handleAdd}>+ Create Order</button>
          </div>

          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Order Number</th>
                <th>Supplier</th>
                <th>Total Amount</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td>{order.id}</td>
                  <td>{order.orderNumber}</td>
                  <td>{order.supplierId}</td>
                  <td>{formatCurrency(order.totalAmount)}</td>
                  <td>
                    <span className={`status ${order.status.toLowerCase()}`}>
                      {order.status}
                    </span>
                  </td>
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
              <h2>{orderDetail ? 'Order Details' : 'Create New Order'}</h2>
              <button className="close-button" onClick={handleCancel}>×</button>
            </div>
            <div className="modal-body">
              {loadingDetail ? (
                <div className="loading">Loading order details...</div>
              ) : orderDetail ? (
                <div className="order-detail">
                  <div className="order-info-section">
                    <h3>Order Information</h3>
                    <div className="info-grid">
                      <div className="info-item">
                        <label>Order Number:</label>
                        <span>{orderDetail.orderNumber}</span>
                      </div>
                      <div className="info-item">
                        <label>Supplier:</label>
                        <SupplierSearch
                          value={orderDetail.supplierData || null}
                          onChange={handleSupplierChange}
                          disabled={isReadOnly}
                          hasError={!!fieldErrors.supplierId}
                        />
                        {fieldErrors.supplierId && (
                          <span className="field-error">{fieldErrors.supplierId}</span>
                        )}
                      </div>
                      <div className="info-item">
                        <label>Status:</label>
                        <span className={`status ${orderDetail.status.toLowerCase()}`}>{orderDetail.status}</span>
                      </div>
                    </div>
                    <div className="form-group">
                      <label>Notes:</label>
                      <textarea
                        value={orderDetail.notes || ''}
                        onChange={(e) => setOrderDetail({ ...orderDetail, notes: e.target.value })}
                        placeholder="Add notes..."
                        rows={3}
                        disabled={isReadOnly}
                      />
                    </div>
                  </div>

                  <div className="order-lines-section">
                    <h3>Order Lines</h3>
                    <table className="data-table editable-table">
                      <thead>
                        <tr>
                          <th>Product</th>
                          <th>Qty Ordered</th>
                          <th>Qty Received</th>
                          <th>Qty Remaining</th>
                          <th>Unit Price</th>
                          <th>Line Total</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {editableLines.map((line, index) => (
                          <tr key={line.id}>
                            <td>{line.productName}</td>
                            <td>
                              <input
                                type="number"
                                value={line.quantityOrdered}
                                onChange={(e) => handleLineChange(index, 'quantityOrdered', parseFloat(e.target.value) || 0)}
                                disabled={isReadOnly}
                                min="0"
                                step="0.01"
                                className="table-input"
                              />
                            </td>
                            <td>{line.quantityReceived}</td>
                            <td>{line.quantityRemaining}</td>
                            <td>
                              <input
                                type="number"
                                value={line.unitPrice}
                                onChange={(e) => handleLineChange(index, 'unitPrice', parseFloat(e.target.value) || 0)}
                                disabled={isReadOnly}
                                min="0"
                                step="0.01"
                                className="table-input"
                              />
                            </td>
                            <td className="line-total">{formatCurrency(line.lineTotal)}</td>
                            <td>
                              {!isReadOnly && (
                                <button
                                  className="action-btn delete"
                                  onClick={() => handleRemoveLine(index)}
                                  style={{ padding: '4px 8px', fontSize: '12px' }}
                                >
                                  Remove
                                </button>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                    {!isReadOnly && (
                      <button className="add-button" onClick={handleAddLine} style={{ marginTop: '10px' }}>
                        + Add Line
                      </button>
                    )}
                  </div>

                  <div className="order-totals-footer">
                    <div className="totals-grid">
                      <div className="total-item total-amount-item">
                        <label>Total Amount:</label>
                        <span className="grand-total">{formatCurrency(orderDetail.totalAmount || 0)}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ) : isCreating ? (
                <div className="order-detail">
                  <div className="order-info-section">
                    <h3>New Purchase Order</h3>
                    <div className="info-grid">
                      <div className="info-item">
                        <label>Supplier:</label>
                        <SupplierSearch
                          value={createSupplier}
                          onChange={setCreateSupplier}
                          hasError={!!fieldErrors.supplierId}
                        />
                      </div>
                    </div>
                    <div className="form-group">
                      <label>Notes:</label>
                      <textarea
                        value={createNotes}
                        onChange={(e) => setCreateNotes(e.target.value)}
                        placeholder="Add notes..."
                        rows={3}
                      />
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
                          <th>Line Total</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {editableLines.map((line, index) => (
                          <tr key={line.id}>
                            <td>
                              <ProductSearch
                                value={line.productData ?? null}
                                onChange={(product) => handleCreateLineProductChange(index, product)}
                              />
                            </td>
                            <td>
                              <input
                                type="number"
                                value={line.quantityOrdered}
                                onChange={(e) => handleLineChange(index, 'quantityOrdered', parseFloat(e.target.value) || 0)}
                                min="0"
                                step="0.01"
                                className="table-input"
                              />
                            </td>
                            <td>
                              <input
                                type="number"
                                value={line.unitPrice}
                                onChange={(e) => handleLineChange(index, 'unitPrice', parseFloat(e.target.value) || 0)}
                                min="0"
                                step="0.01"
                                className="table-input"
                              />
                            </td>
                            <td className="line-total">{formatCurrency(line.lineTotal)}</td>
                            <td>
                              <button
                                className="action-btn delete"
                                onClick={() => handleRemoveLine(index)}
                                style={{ padding: '4px 8px', fontSize: '12px' }}
                              >
                                Remove
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                    <button className="add-button" onClick={handleAddLine} style={{ marginTop: '10px' }}>
                      + Add Line
                    </button>
                  </div>
                  <div className="order-totals-footer">
                    <div className="totals-grid">
                      <div className="total-item total-amount-item">
                        <label>Total Amount:</label>
                        <span className="grand-total">
                          {formatCurrency(editableLines.reduce((sum, line) => sum + line.lineTotal, 0))}
                        </span>
                      </div>
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
                  onClick={handleOpenGoodsReceipt}
                  disabled={goodsReceipt.isGoodsReceiptButtonDisabled(orderDetail.status)}
                  title={
                    goodsReceipt.isGoodsReceiptButtonDisabled(orderDetail.status)
                      ? 'Purchase order must be approved before creating goods receipt'
                      : 'Open goods receipt'
                  }
                >
                  Goods Receipt
                </button>
                <button className="approve-button" onClick={handleApprove} disabled={orderDetail.status !== 'Draft'}>
                  Approve
                </button>
                <button className="save-button" onClick={handleSave}>Save Changes</button>
              </div>
            )}
          </div>
        </div>
      )}

      {toast && (
        <div className={`toast toast-${toast.type}`}>
          {toast.message}
        </div>
      )}

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
        formatCurrency={formatCurrency}
      />

      <GoodsReceiptSearchModal
        isOpen={goodsReceipt.showSearchModal}
        purchaseOrderId={goodsReceipt.activePurchaseOrder?.id}
        poStatus={goodsReceipt.activePurchaseOrder?.status}
        receipts={goodsReceipt.goodsReceiptList}
        onClose={goodsReceipt.closeSearchModal}
        onSelect={goodsReceipt.handleSelectGoodsReceipt}
        onCreateNew={goodsReceipt.handleCreateNewGoodsReceipt}
        formatDate={formatDate}
      />
    </div>
  );
}
