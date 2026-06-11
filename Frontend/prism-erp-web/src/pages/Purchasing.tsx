import { useState, useEffect } from 'react';
import { suppliersApi, purchaseOrdersApi, goodsReceiptsApi } from '../services/purchasingApi';
import type { PurchaseOrderSummaryDto, PurchaseOrderDto, SupplierDto, GoodsReceiptDto, GoodsReceiptSummaryDto } from '../services/types/purchasing.types';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import SupplierSearch from '../components/SupplierSearch';
import './Inventory.css';
import './Purchasing.css';

interface PurchaseOrder extends PurchaseOrderSummaryDto {

}

interface OrderDetail extends PurchaseOrderDto {
  supplierData?: SupplierDto;
  supplierName?: string;
  totalAmount?: number;
}

interface OrderLineEditable {
  id: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityReceived: number;
  quantityRemaining: number;
  unitPrice: number;
  lineTotal: number;
}

interface GoodsReceiptLineEditable {
  id: number;
  purchaseOrderLineId: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitCost: number;
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
  
  // Goods Receipt states
  const [showGoodsReceiptModal, setShowGoodsReceiptModal] = useState(false);
  const [goodsReceipt, setGoodsReceipt] = useState<GoodsReceiptDto | null>(null);
  const [goodsReceiptLines, setGoodsReceiptLines] = useState<GoodsReceiptLineEditable[]>([]);
  const [loadingGoodsReceipt, setLoadingGoodsReceipt] = useState(false);
  const [showGoodsReceiptListModal, setShowGoodsReceiptListModal] = useState(false);
  const [goodsReceiptList, setGoodsReceiptList] = useState<GoodsReceiptSummaryDto[]>([]);
  const [selectedPurchaseOrder, setSelectedPurchaseOrder] = useState<PurchaseOrder | null>(null);

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const data = await purchaseOrdersApi.getAll();
        const mappedOrders: PurchaseOrder[] = data.map(dto => ({
          ...dto
        }));
        setOrders(mappedOrders);
      } catch (error) {
        console.log('API not available, using mock data');
        setOrders([]);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, []);

  const handleAdd = () => {
    setOrderDetail(null);
    setShowModal(true);
  };

  const handleEdit = async (order: PurchaseOrder) => {
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const data = await purchaseOrdersApi.getById(order.id);
      const supplierData = await suppliersApi.getById(order.supplierId);
      const editableLines: OrderLineEditable[] = data.lines.map(line => ({
        ...line,
        productName: `Product ${line.productId}`,
        lineTotal: line.quantityOrdered * line.unitPrice,
      }));
      setEditableLines(editableLines);
      setOrderDetail({
        ...data,
        supplierData: supplierData,
        supplierName: supplierData.name,
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
      setToast({ message: 'Order approved successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      setShowModal(false);
      const data = await purchaseOrdersApi.getAll();
      const mappedOrders: PurchaseOrder[] = data.map(dto => ({
        ...dto,
      }));
      setOrders(mappedOrders);
    } catch (error: any) {
      const apiError = parseApiError(error);
      setToast({ message: getToastMessage(apiError), type: 'error' });
      setTimeout(() => setToast(null), 5000);
    }
  };

  const handleCancelOrder = async () => {
    if (!orderDetail) return;
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await purchaseOrdersApi.cancel(orderDetail.id);
        setToast({ message: 'Order cancelled successfully!', type: 'success' });
        setTimeout(() => setToast(null), 3000);
        setShowModal(false);
        const data = await purchaseOrdersApi.getAll();
        const mappedOrders: PurchaseOrder[] = data.map(dto => ({
          ...dto,
        }));
        setOrders(mappedOrders);
      } catch (error: any) {
        const apiError = parseApiError(error);
        setToast({ message: getToastMessage(apiError), type: 'error' });
        setTimeout(() => setToast(null), 5000);
      }
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to cancel this order?')) {
      try {
        await purchaseOrdersApi.cancel(id);
        setToast({ message: 'Order cancelled successfully!', type: 'success' });
        setTimeout(() => setToast(null), 3000);
        setOrders(orders.filter(order => order.id !== id));
      } catch (error: any) {
        const apiError = parseApiError(error);
        setToast({ message: getToastMessage(apiError), type: 'error' });
        setTimeout(() => setToast(null), 5000);
      }
    }
  };

  const handleApproveInline = async (id: number) => {
    try {
      await purchaseOrdersApi.approve(id);
      setToast({ message: 'Order approved successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      const data = await purchaseOrdersApi.getAll();
      const mappedOrders: PurchaseOrder[] = data.map(dto => ({
        ...dto,
      }));
      setOrders(mappedOrders);
    } catch (error: any) {
      const apiError = parseApiError(error);
      setToast({ message: getToastMessage(apiError), type: 'error' });
      setTimeout(() => setToast(null), 5000);
    }
  };

  const handleViewGoodsReceipt = async (order: PurchaseOrder) => {
    const status = order.status.toLowerCase();
    
    // Draft or Cancel - cannot create goods receipt
    if (status === 'draft' || status === 'cancel') {
      setToast({ message: 'Purchase order must be approved before creating goods receipt', type: 'error' });
      setTimeout(() => setToast(null), 3000);
      return;
    }
    
    // Approved - check if there's an existing Draft GR
    if (status === 'approved') {
      setSelectedPurchaseOrder(order);
      setLoadingGoodsReceipt(true);
      setShowGoodsReceiptModal(true);
      try {
        const receipts = await goodsReceiptsApi.getByPurchaseOrder(order.id);
        const draftReceipt = receipts.find(r => r.status.toLowerCase() === 'draft');
        
        if (draftReceipt) {
          // Open existing Draft GR
          const receipt = await goodsReceiptsApi.getById(draftReceipt.id);
          const editableLines: GoodsReceiptLineEditable[] = receipt.lines.map(line => ({
            id: line.id,
            purchaseOrderLineId: line.purchaseOrderLineId,
            productId: line.productId,
            productName: `Product ${line.productId}`,
            quantityOrdered: 0,
            quantityReceived: line.quantity,
            unitCost: line.unitCost,
            lineTotal: line.quantity * line.unitCost,
          }));
          setGoodsReceiptLines(editableLines);
          setGoodsReceipt(receipt);
        } else {
          // Create new GR - parse all PO details
          const poDetail = await purchaseOrdersApi.getById(order.id);
          const editableLines: GoodsReceiptLineEditable[] = poDetail.lines.map(line => ({
            id: 0,
            purchaseOrderLineId: line.id,
            productId: line.productId,
            productName: `Product ${line.productId}`,
            quantityOrdered: line.quantityOrdered,
            quantityReceived: 0,
            unitCost: line.unitPrice,
            lineTotal: 0,
          }));
          setGoodsReceiptLines(editableLines);
          setGoodsReceipt({
            id: 0,
            receiptNumber: `GR-${new Date().getFullYear()}-${String(order.id).padStart(4, '0')}`,
            purchaseOrderId: order.id,
            status: 'Draft',
            postedAt: undefined,
            notes: '',
            lines: [],
          });
        }
      } catch (error) {
        console.log('API not available, using mock goods receipt data');
        setGoodsReceiptLines([]);
        setGoodsReceipt(null);
      } finally {
        setLoadingGoodsReceipt(false);
      }
      return;
    }
    
    // Received or PartialReceived - open search/list modal
    if (status === 'received' || status === 'partialreceived') {
      setSelectedPurchaseOrder(order);
      setShowGoodsReceiptListModal(true);
      try {
        const receipts = await goodsReceiptsApi.getByPurchaseOrder(order.id);
        setGoodsReceiptList(receipts);
      } catch (error) {
        console.log('API not available, using mock goods receipt list');
        setGoodsReceiptList([]);
      }
    }
  };

  const handleSelectGoodsReceipt = async (receiptId: number) => {
    setShowGoodsReceiptListModal(false);
    setLoadingGoodsReceipt(true);
    setShowGoodsReceiptModal(true);
    try {
      const receipt = await goodsReceiptsApi.getById(receiptId);
      
      // For PartialReceived/Received PO, show only 1 empty line for user input
      const editableLines: GoodsReceiptLineEditable[] = [{
        id: Date.now(),
        purchaseOrderLineId: 0,
        productId: 0,
        productName: '',
        quantityOrdered: 0,
        quantityReceived: 0,
        unitCost: 0,
        lineTotal: 0,
      }];
      
      setGoodsReceiptLines(editableLines);
      setGoodsReceipt(receipt);
    } catch (error) {
      console.log('API not available, using mock goods receipt data');
      setGoodsReceiptLines([]);
      setGoodsReceipt(null);
    } finally {
      setLoadingGoodsReceipt(false);
    }
  };

  const handleCreateNewGoodsReceipt = async () => {
    if (!selectedPurchaseOrder) return;
    setShowGoodsReceiptListModal(false);
    setLoadingGoodsReceipt(true);
    setShowGoodsReceiptModal(true);
    try {
      const existingReceipts = await goodsReceiptsApi.getByPurchaseOrder(selectedPurchaseOrder.id);
      
      // For PartialReceived/Received PO, create empty line for user input
      const editableLines: GoodsReceiptLineEditable[] = [{
        id: Date.now(),
        purchaseOrderLineId: 0,
        productId: 0,
        productName: '',
        quantityOrdered: 0,
        quantityReceived: 0,
        unitCost: 0,
        lineTotal: 0,
      }];
      
      setGoodsReceiptLines(editableLines);
      setGoodsReceipt({
        id: 0,
        receiptNumber: `GR-${new Date().getFullYear()}-${String(selectedPurchaseOrder.id).padStart(4, '0')}-${String(existingReceipts.length + 1).padStart(2, '0')}`,
        purchaseOrderId: selectedPurchaseOrder.id,
        status: 'Draft',
        postedAt: undefined,
        notes: '',
        lines: [],
      });
    } catch (error) {
      console.log('API not available, using mock goods receipt data');
      setGoodsReceiptLines([]);
      setGoodsReceipt(null);
    } finally {
      setLoadingGoodsReceipt(false);
    }
  };

  const handleCancelGoodsReceiptList = () => {
    setShowGoodsReceiptListModal(false);
    setGoodsReceiptList([]);
    setSelectedPurchaseOrder(null);
  };

  const handleGoodsReceiptLineChange = (index: number, field: keyof GoodsReceiptLineEditable, value: number) => {
    const updatedLines = [...goodsReceiptLines];
    updatedLines[index] = { ...updatedLines[index], [field]: value };
    updatedLines[index].lineTotal = updatedLines[index].quantityReceived * updatedLines[index].unitCost;
    setGoodsReceiptLines(updatedLines);
  };

  const handleAddGoodsReceiptLine = () => {
    const newLine: GoodsReceiptLineEditable = {
      id: Date.now(),
      purchaseOrderLineId: 0,
      productId: 0,
      productName: '',
      quantityOrdered: 0,
      quantityReceived: 0,
      unitCost: 0,
      lineTotal: 0,
    };
    setGoodsReceiptLines([...goodsReceiptLines, newLine]);
  };

  const handleRemoveGoodsReceiptLine = (index: number) => {
    const updatedLines = goodsReceiptLines.filter((_, i) => i !== index);
    setGoodsReceiptLines(updatedLines);
  };

  const handleSaveGoodsReceipt = async () => {
    if (!goodsReceipt) return;
    
    try {
      if (goodsReceipt.id === 0) {
        // Create new
        await goodsReceiptsApi.create({
          purchaseOrderId: goodsReceipt.purchaseOrderId,
          notes: goodsReceipt.notes,
          lines: goodsReceiptLines.map(line => ({
            purchaseOrderLineId: line.purchaseOrderLineId,
            quantity: line.quantityReceived,
            unitCost: line.unitCost,
          })),
          postImmediately: false,
        });
        setToast({ message: 'Goods receipt created successfully!', type: 'success' });
      } else {
        // Update existing
        await goodsReceiptsApi.update(goodsReceipt.id, {
          notes: goodsReceipt.notes,
          lines: goodsReceiptLines.map(line => ({
            purchaseOrderLineId: line.purchaseOrderLineId,
            quantity: line.quantityReceived,
            unitCost: line.unitCost,
          })),
        });
        setToast({ message: 'Goods receipt updated successfully!', type: 'success' });
      }
      setTimeout(() => setToast(null), 3000);
      setShowGoodsReceiptModal(false);
    } catch (error: any) {
      const apiError = parseApiError(error);
      setToast({ message: getToastMessage(apiError), type: 'error' });
      setTimeout(() => setToast(null), 5000);
    }
  };

  const handlePostGoodsReceipt = async () => {
    if (!goodsReceipt || goodsReceipt.id === 0) return;
    
    try {
      await goodsReceiptsApi.post(goodsReceipt.id);
      setToast({ message: 'Goods receipt posted successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      setShowGoodsReceiptModal(false);
    } catch (error: any) {
      const apiError = parseApiError(error);
      setToast({ message: getToastMessage(apiError), type: 'error' });
      setTimeout(() => setToast(null), 5000);
    }
  };

  const handleCancelGoodsReceipt = () => {
    setShowGoodsReceiptModal(false);
    setGoodsReceipt(null);
    setGoodsReceiptLines([]);
    setSelectedPurchaseOrder(null);
  };

  const calculateLineTotals = (lines: OrderLineEditable[]): OrderLineEditable[] => {
    return lines.map(line => ({
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

  const handleSave = async () => {
    if (!orderDetail) return;
    
    setFieldErrors({});
    
    try {
      await purchaseOrdersApi.update(orderDetail.id, {
        supplierId: orderDetail.supplierId,
        warehouseId: orderDetail.warehouseId,
        notes: orderDetail.notes,
        lines: editableLines.map(line => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
        })),
      });
      setToast({ message: 'Order updated successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      setShowModal(false);
      const data = await purchaseOrdersApi.getAll();
      const mappedOrders: PurchaseOrder[] = data.map(dto => ({
        ...dto,
      }));
      setOrders(mappedOrders);
    } catch (error: any) {
      const apiError = parseApiError(error);
      
      console.log('API Error:', apiError);
      
      if (apiError.type === 'validation-error' && apiError.fieldErrors) {
        const errors: Record<string, string> = {};
        apiError.fieldErrors.forEach(fe => {
          const fieldName = fe.field.toLowerCase();
          if (fieldName.includes('supplier') && fieldName.includes('id')) {
            errors.supplierId = fe.messages[0];
          } else if (fieldName.includes('supplier')) {
            errors.supplierId = fe.messages[0];
          } else {
            errors[fe.field] = fe.messages[0];
          }
        });
        console.log('Field errors:', errors);
        setFieldErrors(errors);
        
        setTimeout(() => setFieldErrors({}), 2000);
      }
      
      setToast({ message: getToastMessage(apiError), type: 'error' });
      setTimeout(() => setToast(null), 2000);
    }
  };

  const handleCancel = () => {
    setShowModal(false);
    setOrderDetail(null);
    setEditableLines([]);
    setFieldErrors({});
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
    if(!supplier && orderDetail) {
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
                    <button className="action-btn approve" onClick={() => handleApproveInline(order.id)}>Approve</button>
                    <button className="action-btn goods-receipt" onClick={() => handleViewGoodsReceipt(order)}>Goods Receipt</button>
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
              ) : (
                <div className="form-group">
                  <label>Supplier</label>
                  <input type="text" placeholder="Enter supplier name" />
                  <p>Create order form will be implemented here</p>
                </div>
              )}
            </div>
            {orderDetail && (
              <div className="modal-footer">
                <button className="cancel-button" onClick={handleCancelOrder}>Cancel Order</button>
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

      {showGoodsReceiptModal && (
        <div className="modal-overlay">
          <div className="modal modal-large">
            <div className="modal-header">
              <h2>Goods Receipt</h2>
              <button className="close-button" onClick={handleCancelGoodsReceipt}>×</button>
            </div>
            <div className="modal-body">
              {loadingGoodsReceipt ? (
                <div className="loading">Loading goods receipt...</div>
              ) : goodsReceipt ? (
                <div className="order-detail">
                  <div className="order-info-section">
                    <h3>Receipt Information</h3>
                    <div className="info-grid">
                      <div className="info-item">
                        <label>Receipt Number:</label>
                        <span>{goodsReceipt.receiptNumber}</span>
                      </div>
                      <div className="info-item">
                        <label>Purchase Order ID:</label>
                        <span>{goodsReceipt.purchaseOrderId}</span>
                      </div>
                      <div className="info-item">
                        <label>Status:</label>
                        <span className={`status ${goodsReceipt.status.toLowerCase()}`}>{goodsReceipt.status}</span>
                      </div>
                    </div>
                    <div className="form-group">
                      <label>Notes:</label>
                      <textarea
                        value={goodsReceipt.notes || ''}
                        onChange={(e) => setGoodsReceipt({ ...goodsReceipt, notes: e.target.value })}
                        placeholder="Add notes..."
                        rows={3}
                        disabled={goodsReceipt.status !== 'Draft'}
                      />
                    </div>
                  </div>

                  <div className="order-lines-section">
                    <h3>Receipt Lines</h3>
                    <table className="data-table editable-table">
                      <thead>
                        <tr>
                          <th>Product</th>
                          <th>Qty Ordered</th>
                          <th>Qty Received</th>
                          <th>Unit Cost</th>
                          <th>Line Total</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {goodsReceiptLines.map((line, index) => (
                          <tr key={line.id || index}>
                            <td>{line.productName}</td>
                            <td>{line.quantityOrdered}</td>
                            <td>
                              <input
                                type="number"
                                value={line.quantityReceived}
                                onChange={(e) => handleGoodsReceiptLineChange(index, 'quantityReceived', parseFloat(e.target.value) || 0)}
                                min="0"
                                max={line.quantityOrdered}
                                step="0.01"
                                className="table-input"
                                disabled={goodsReceipt.status !== 'Draft'}
                              />
                            </td>
                            <td>
                              <input
                                type="number"
                                value={line.unitCost}
                                onChange={(e) => handleGoodsReceiptLineChange(index, 'unitCost', parseFloat(e.target.value) || 0)}
                                min="0"
                                step="0.01"
                                className="table-input"
                                disabled={goodsReceipt.status !== 'Draft'}
                              />
                            </td>
                            <td className="line-total">{formatCurrency(line.lineTotal)}</td>
                            <td>
                              {goodsReceipt.status === 'Draft' && (
                                <button 
                                  className="action-btn delete" 
                                  onClick={() => handleRemoveGoodsReceiptLine(index)}
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
                    {goodsReceipt.status === 'Draft' && (
                      <button className="add-button" onClick={handleAddGoodsReceiptLine} style={{ marginTop: '10px' }}>
                        + Add Line
                      </button>
                    )}
                  </div>

                  <div className="order-totals-footer">
                    <div className="totals-grid">
                      <div className="total-item total-amount-item">
                        <label>Total Amount:</label>
                        <span className="grand-total">{formatCurrency(goodsReceiptLines.reduce((sum, line) => sum + line.lineTotal, 0))}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ) : null}
            </div>
            {goodsReceipt && (
              <div className="modal-footer">
                <button className="cancel-button" onClick={handleCancelGoodsReceipt}>Cancel</button>
                {goodsReceipt.id !== 0 && goodsReceipt.status === 'Draft' && (
                  <button className="approve-button" onClick={handlePostGoodsReceipt}>Post</button>
                )}
                {goodsReceipt.status === 'Draft' && (
                  <button className="save-button" onClick={handleSaveGoodsReceipt}>Save</button>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {showGoodsReceiptListModal && (
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h2>Goods Receipts for PO #{selectedPurchaseOrder?.id}</h2>
              <button className="close-button" onClick={handleCancelGoodsReceiptList}>×</button>
            </div>
            <div className="modal-body">
              <div className="data-table-container">
                <div className="table-header">
                  <h2>Goods Receipt List</h2>
                  <button className="add-button" onClick={handleCreateNewGoodsReceipt}>+ Create New Receipt</button>
                </div>
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Receipt Number</th>
                      <th>Status</th>
                      <th>Posted Date</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {goodsReceiptList.map((receipt) => (
                      <tr key={receipt.id}>
                        <td>{receipt.id}</td>
                        <td>{receipt.receiptNumber}</td>
                        <td>
                          <span className={`status ${receipt.status.toLowerCase()}`}>
                            {receipt.status}
                          </span>
                        </td>
                        <td>{receipt.postedAt ? formatDate(receipt.postedAt) : 'N/A'}</td>
                        <td>
                          <button className="action-btn edit" onClick={() => handleSelectGoodsReceipt(receipt.id)}>View</button>
                        </td>
                      </tr>
                    ))}
                    {goodsReceiptList.length === 0 && (
                      <tr>
                        <td colSpan={5} style={{ textAlign: 'center', padding: '20px' }}>
                          No goods receipts found for this purchase order
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
