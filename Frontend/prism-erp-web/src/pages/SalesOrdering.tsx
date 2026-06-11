import { useState, useEffect } from 'react';
import { customersApi, salesOrdersApi } from '../services/salesOrderingApi';
import type { SalesOrderSummaryDto, SalesOrderDto, CustomerDto } from '../services/types/salesOrdering.types';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import CustomerSearch from '../components/CustomerSearch';
import './Inventory.css';
import './SalesOrdering.css';

interface SalesOrder extends SalesOrderSummaryDto {

}

interface OrderDetail extends SalesOrderDto {
  customerData?: CustomerDto;
}

interface OrderLineEditable {
  id: number;
  productId: number;
  productName: string;
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

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const data = await salesOrdersApi.getAll();
        // Map SalesOrderSummaryDto to SalesOrder interface (customerName already exists)
        const mappedOrders: SalesOrder[] = data.map(dto => ({
          ...dto
        }));
        setOrders(mappedOrders);
      } catch (error) {
        console.log('API not available, using mock data');
        // Fallback to mock data
        setOrders([
          { id: 1, orderNumber: 'SO-2024-001', totalAmount: 2500000, status: 'Draft', orderDate: '2024-01-15', customerId: 1, customerName: 'Sen Vang Restaurant', deliveryDate: undefined, approvedAt: undefined, notes: undefined, subTotal: 0, totalDiscount: 0, totalVAT: 0 },
          { id: 2, orderNumber: 'SO-2024-002', totalAmount: 1800000, status: 'Confirmed', orderDate: '2024-01-16', customerId: 2, customerName: 'Com Nieu Restaurant', deliveryDate: undefined, approvedAt: undefined, notes: undefined, subTotal: 0, totalDiscount: 0, totalVAT: 0 },
          { id: 3, orderNumber: 'SO-2024-003', totalAmount: 3200000, status: 'Delivered', orderDate: '2024-01-17', customerId: 3, customerName: 'Riverside Hotel', deliveryDate: undefined, approvedAt: undefined, notes: undefined, subTotal: 0, totalDiscount: 0, totalVAT: 0 },
        ]);
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

  const handleEdit = async (order: SalesOrder) => {
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const data = await salesOrdersApi.getById(order.id);
      const customerData = await customersApi.getById(order.customerId);
      const editableLines: OrderLineEditable[] = data.lines.map(line => ({
        ...line,
      }));
      setEditableLines(editableLines);
      setOrderDetail({
        ...data,
        customerData: customerData,
      });
    } catch (error) {
      console.log('API not available, using mock detail data');
      // Mock detail data
      const mockLines: OrderLineEditable[] = [
        {
          id: 1,
          productId: 1,
          productName: 'Product A',
          quantityOrdered: 10,
          quantityDelivered: 0,
          quantityRemaining: 10,
          unitPrice: 100000,
          discountPercent: 0,
          vatRate: 10,
          discountAmount: 0,
          vatAmount: 100000,
          lineTotal: 1100000,
        },
        {
          id: 2,
          productId: 2,
          productName: 'Product B',
          quantityOrdered: 5,
          quantityDelivered: 0,
          quantityRemaining: 5,
          unitPrice: 200000,
          discountPercent: 5,
          vatRate: 10,
          discountAmount: 50000,
          vatAmount: 95000,
          lineTotal: 1045000,
        },
      ];
      setEditableLines(mockLines);
      setOrderDetail({
        id: order.id,
        orderNumber: order.orderNumber,
        customerId: order.customerId,
        customerName: order.customerName,
        orderDate: order.orderDate,
        deliveryDate: order.deliveryDate,
        approvedAt: order.approvedAt,
        status: order.status,
        notes: 'Sample notes for the order',
        subTotal: order.totalAmount * 0.9,
        totalDiscount: order.totalAmount * 0.1,
        totalVAT: order.totalAmount * 0.1,
        totalAmount: order.totalAmount,
        lines: mockLines
      });
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleApprove = async () => {
    if (!orderDetail) return;
    try {
      await handleSave();
      await salesOrdersApi.approve(orderDetail.id);
      setToast({ message: 'Order approved successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      setShowModal(false);
      // Refresh the list
      const data = await salesOrdersApi.getAll();
      const mappedOrders: SalesOrder[] = data.map(dto => ({
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
        await salesOrdersApi.cancel(orderDetail.id);
        setToast({ message: 'Order cancelled successfully!', type: 'success' });
        setTimeout(() => setToast(null), 3000);
        setShowModal(false);
        // Refresh the list
        const data = await salesOrdersApi.getAll();
        const mappedOrders: SalesOrder[] = data.map(dto => ({
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
        await salesOrdersApi.cancel(id);
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
      await salesOrdersApi.approve(id);
      setToast({ message: 'Order approved successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      // Refresh the list
      const data = await salesOrdersApi.getAll();
      const mappedOrders: SalesOrder[] = data.map(dto => ({
        ...dto,
      }));
      setOrders(mappedOrders);
    } catch (error: any) {
      const apiError = parseApiError(error);
      setToast({ message: getToastMessage(apiError), type: 'error' });
      setTimeout(() => setToast(null), 5000);
    }
  };

  const calculateLineTotals = (lines: OrderLineEditable[]): OrderLineEditable[] => {
    return lines.map(line => {
      const gross = line.unitPrice * line.quantityOrdered;
      const discountAmount = gross * (line.discountPercent / 100);
      const afterDiscount = gross - discountAmount;
      const vatAmount = afterDiscount * (line.vatRate / 100);
      const lineTotal = afterDiscount + vatAmount;
      return {
        ...line,
        discountAmount,
        vatAmount,
        lineTotal,
      };
    });
  };

  const calculateOrderTotals = (lines: OrderLineEditable[]) => {
    const subTotal = lines.reduce((sum, line) => sum + (line.unitPrice * line.quantityOrdered), 0);
    const totalDiscount = lines.reduce((sum, line) => sum + line.discountAmount, 0);
    const totalVAT = lines.reduce((sum, line) => sum + line.vatAmount, 0);
    const totalAmount = lines.reduce((sum, line) => sum + line.lineTotal, 0);
    return { subTotal, totalDiscount, totalVAT, totalAmount };
  };

  const handleLineChange = (index: number, field: keyof OrderLineEditable, value: number) => {
    const updatedLines = [...editableLines];
    updatedLines[index] = { ...updatedLines[index], [field]: value };
    const recalculatedLines = calculateLineTotals(updatedLines);
    setEditableLines(recalculatedLines);

    // Update order totals
    const totals = calculateOrderTotals(recalculatedLines);
    if (orderDetail) {
      setOrderDetail({
        ...orderDetail,
        subTotal: totals.subTotal,
        totalDiscount: totals.totalDiscount,
        totalVAT: totals.totalVAT,
        totalAmount: totals.totalAmount,
        lines: recalculatedLines,
      });
    }
  };

  const handleSave = async () => {
    if (!orderDetail) return;
    
    // Clear previous field errors
    setFieldErrors({});
    
    try {
      await salesOrdersApi.update(orderDetail.id, {
        customerId: orderDetail.customerId,
        customerName: orderDetail.customerName,
        notes: orderDetail.notes,
        lines: editableLines.map(line => ({
          productId: line.productId,
          productName: line.productName,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
          discountPercent: line.discountPercent,
          vatRate: line.vatRate,
        })),
      });
      setToast({ message: 'Order updated successfully!', type: 'success' });
      setTimeout(() => setToast(null), 3000);
      setShowModal(false);
      // Refresh the list
      const data = await salesOrdersApi.getAll();
      const mappedOrders: SalesOrder[] = data.map(dto => ({
        ...dto,
      }));
      setOrders(mappedOrders);
    } catch (error: any) {
      const apiError = parseApiError(error);
      
      console.log('API Error:', apiError);
      
      // Handle field-level validation errors
      if (apiError.type === 'validation-error' && apiError.fieldErrors) {
        const errors: Record<string, string> = {};
        apiError.fieldErrors.forEach(fe => {
          // Map backend field names to UI field names
          const fieldName = fe.field.toLowerCase();
          if (fieldName.includes('customer') && fieldName.includes('id')) {
            errors.customerId = fe.messages[0];
          } else if (fieldName.includes('customer')) {
            errors.customerId = fe.messages[0];
          } else {
            errors[fe.field] = fe.messages[0];
          }
        });
        console.log('Field errors:', errors);
        setFieldErrors(errors);
        
        // Clear field errors after 2 seconds
        setTimeout(() => setFieldErrors({}), 2000);
      }
      
      // Show toast for all error types
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

  const handleCustomerChange = (customer: CustomerDto | null) => {
    if (customer && orderDetail) {
      setOrderDetail({
        ...orderDetail,
        customerId: customer.id,
        customerName: customer.name,
        customerData: customer,
      });
    }
    if(!customer && orderDetail) {
      setOrderDetail({
        ...orderDetail,
        customerId: 0,
        customerName: '',
        customerData: undefined,
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
        <h1>📋 Sales Ordering Module</h1>
        <p>Manage sales orders</p>
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
                  <td>
                    <span className={`status ${order.status.toLowerCase()}`}>
                      {order.status}
                    </span>
                  </td>
                  <td>{formatDate(order.orderDate)}</td>
                  <td>
                    <button className="action-btn edit" onClick={() => handleEdit(order)}>Edit</button>
                    <button className="action-btn approve" onClick={() => handleApproveInline(order.id)}>Approve</button>
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
                        <label>Customer:</label>
                        <CustomerSearch
                          value={orderDetail.customerData || null}
                          onChange={handleCustomerChange}
                          disabled={isReadOnly}
                          hasError={!!fieldErrors.customerId}
                        />
                        {fieldErrors.customerId && (
                          <span className="field-error">{fieldErrors.customerId}</span>
                        )}
                      </div>
                      <div className="info-item">
                        <label>Order Date:</label>
                        <span>{formatDate(orderDetail.orderDate)}</span>
                      </div>
                      <div className="info-item">
                        <label>Delivery Date:</label>
                        <span>{formatDate(orderDetail.deliveryDate)}</span>
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
                          <th>Qty Delivered</th>
                          <th>Qty Remaining</th>
                          <th>Unit Price</th>
                          <th>Discount %</th>
                          <th>VAT %</th>
                          <th>Line Total</th>
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
                            <td>{line.quantityDelivered}</td>
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
                            <td>
                              <input
                                type="number"
                                value={line.discountPercent}
                                onChange={(e) => handleLineChange(index, 'discountPercent', parseFloat(e.target.value) || 0)}
                                disabled={isReadOnly}
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
                                disabled={isReadOnly}
                                min="0"
                                max="100"
                                step="0.01"
                                className="table-input"
                              />
                            </td>
                            <td className="line-total">{formatCurrency(line.lineTotal)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div className="order-totals-footer">
                    <div className="totals-grid">
                      <div className="total-item">
                        <label>Sub Total:</label>
                        <span>{formatCurrency(orderDetail.subTotal)}</span>
                      </div>
                      <div className="total-item">
                        <label>Total Discount:</label>
                        <span className="discount-value">-{formatCurrency(orderDetail.totalDiscount)}</span>
                      </div>
                      <div className="total-item">
                        <label>Total VAT:</label>
                        <span>{formatCurrency(orderDetail.totalVAT)}</span>
                      </div>
                      <div className="total-item total-amount-item">
                        <label>Total Amount:</label>
                        <span className="grand-total">{formatCurrency(orderDetail.totalAmount)}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="form-group">
                  <label>Customer</label>
                  <input type="text" placeholder="Enter customer name" />
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
    </div>
  );
}