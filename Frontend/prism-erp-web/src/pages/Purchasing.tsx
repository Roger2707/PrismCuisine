import { useState, useEffect } from 'react';
import { purchaseOrdersApi } from '../services/purchasingApi';
import type { PurchaseOrderSummaryDto } from '../services/types/purchasing.types';
import './Inventory.css';

interface PurchaseOrder extends PurchaseOrderSummaryDto {
  supplier: string;
  orderDate: string;
}

export default function Purchasing() {
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const data = await purchaseOrdersApi.getAll();
        console.log('Fetched purchase orders:', data);
        // Map PurchaseOrderSummaryDto to PurchaseOrder interface with additional fields
        const mappedOrders: PurchaseOrder[] = data.map(dto => ({
          ...dto,
          supplier: 'Supplier Name', // Will be populated from supplier API
          orderDate: dto.approvedAt || new Date().toISOString().split('T')[0],
        }));
        setOrders(mappedOrders);
      } catch (error) {
        console.log('API not available, using mock data');
        // Fallback to mock data
        setOrders([
          { id: 1, orderNumber: 'PO-2024-001', supplier: 'Da Lat Fresh Vegetables Cooperative', totalAmount: 1500000, status: 'Draft', orderDate: '2024-01-15', supplierId: 1, warehouseId: 1, amendedFromPurchaseOrderId: undefined, approvedAt: undefined },
          { id: 2, orderNumber: 'PO-2024-002', supplier: 'An Binh Food Company', totalAmount: 2400000, status: 'Approved', orderDate: '2024-01-16', supplierId: 2, warehouseId: 1, amendedFromPurchaseOrderId: undefined, approvedAt: undefined },
          { id: 3, orderNumber: 'PO-2024-003', supplier: 'Nha Trang Fresh Seafood', totalAmount: 5400000, status: 'Received', orderDate: '2024-01-17', supplierId: 3, warehouseId: 1, amendedFromPurchaseOrderId: undefined, approvedAt: undefined },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, []);

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
            <button className="add-button">+ Create Order</button>
          </div>
          
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Order Number</th>
                <th>Supplier</th>
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
                  <td>{order.supplier}</td>
                  <td>₫{order.totalAmount.toLocaleString()}</td>
                  <td>
                    <span className={`status ${order.status.toLowerCase()}`}>
                      {order.status}
                    </span>
                  </td>
                  <td>{order.orderDate}</td>
                  <td>
                    <button className="action-btn edit">Details</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
