import { useState, useEffect } from 'react';
import { api } from '../services/api';
import './Inventory.css';

interface SalesOrder {
  id: number;
  orderNumber: string;
  customer: string;
  totalAmount: number;
  status: string;
  orderDate: string;
}

interface FormData {
  customer: string;
  totalAmount: string;
  status: string;
}

export default function SalesOrdering() {
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingOrder, setEditingOrder] = useState<SalesOrder | null>(null);
  const [formData, setFormData] = useState<FormData>({
    customer: '',
    totalAmount: '',
    status: 'Draft'
  });

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const data = await api.getSalesOrders();
        setOrders(data);
      } catch (error) {
        console.log('API not available, using mock data');
        // Fallback to mock data
        setOrders([
          { id: 1, orderNumber: 'SO-2024-001', customer: 'Sen Vang Restaurant', totalAmount: 2500000, status: 'Draft', orderDate: '2024-01-15' },
          { id: 2, orderNumber: 'SO-2024-002', customer: 'Com Nieu Restaurant', totalAmount: 1800000, status: 'Confirmed', orderDate: '2024-01-16' },
          { id: 3, orderNumber: 'SO-2024-003', customer: 'Riverside Hotel', totalAmount: 3200000, status: 'Delivered', orderDate: '2024-01-17' },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, []);

  const handleAdd = () => {
    setEditingOrder(null);
    setFormData({ customer: '', totalAmount: '', status: 'Draft' });
    setShowModal(true);
  };

  const handleEdit = (order: SalesOrder) => {
    setEditingOrder(order);
    setFormData({
      customer: order.customer,
      totalAmount: order.totalAmount.toString(),
      status: order.status
    });
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this order?')) {
      try {
        await api.deleteSalesOrder(id);
        setOrders(orders.filter(order => order.id !== id));
      } catch (error) {
        console.log('API not available, using local state');
        setOrders(orders.filter(order => order.id !== id));
      }
    }
  };

  const handleSave = async () => {
    try {
      if (editingOrder) {
        // Try to call API
        await api.updateSalesOrder(editingOrder.id, {
          customer: formData.customer,
          totalAmount: parseFloat(formData.totalAmount),
          status: formData.status
        });
        setOrders(orders.map(order =>
          order.id === editingOrder.id
            ? { ...order, customer: formData.customer, totalAmount: parseFloat(formData.totalAmount), status: formData.status }
            : order
        ));
      } else {
        const newOrder = {
          customer: formData.customer,
          totalAmount: parseFloat(formData.totalAmount),
          status: formData.status
        };
        // Try to call API
        await api.createSalesOrder(newOrder);
        const createdOrder: SalesOrder = {
          id: orders.length + 1,
          orderNumber: `SO-2024-${String(orders.length + 1).padStart(3, '0')}`,
          customer: formData.customer,
          totalAmount: parseFloat(formData.totalAmount),
          status: formData.status,
          orderDate: new Date().toISOString().split('T')[0]
        };
        setOrders([...orders, createdOrder]);
      }
      setShowModal(false);
    } catch (error) {
      console.log('API not available, using local state');
      // Fallback to local state
      if (editingOrder) {
        setOrders(orders.map(order =>
          order.id === editingOrder.id
            ? { ...order, customer: formData.customer, totalAmount: parseFloat(formData.totalAmount), status: formData.status }
            : order
        ));
      } else {
        const newOrder: SalesOrder = {
          id: orders.length + 1,
          orderNumber: `SO-2024-${String(orders.length + 1).padStart(3, '0')}`,
          customer: formData.customer,
          totalAmount: parseFloat(formData.totalAmount),
          status: formData.status,
          orderDate: new Date().toISOString().split('T')[0]
        };
        setOrders([...orders, newOrder]);
      }
      setShowModal(false);
    }
  };

  const handleCancel = () => {
    setShowModal(false);
    setEditingOrder(null);
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
                  <td>{order.customer}</td>
                  <td>₫{order.totalAmount.toLocaleString()}</td>
                  <td>
                    <span className={`status ${order.status.toLowerCase()}`}>
                      {order.status}
                    </span>
                  </td>
                  <td>{order.orderDate}</td>
                  <td>
                    <button className="action-btn edit" onClick={() => handleEdit(order)}>Edit</button>
                    <button className="action-btn delete" onClick={() => handleDelete(order.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h2>{editingOrder ? 'Edit Order' : 'Create New Order'}</h2>
              <button className="close-button" onClick={handleCancel}>×</button>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <label>Customer</label>
                <input
                  type="text"
                  value={formData.customer}
                  onChange={(e) => setFormData({ ...formData, customer: e.target.value })}
                  placeholder="Enter customer name"
                />
              </div>
              <div className="form-group">
                <label>Total Amount</label>
                <input
                  type="number"
                  value={formData.totalAmount}
                  onChange={(e) => setFormData({ ...formData, totalAmount: e.target.value })}
                  placeholder="Enter total amount"
                />
              </div>
              <div className="form-group">
                <label>Status</label>
                <select
                  value={formData.status}
                  onChange={(e) => setFormData({ ...formData, status: e.target.value })}
                >
                  <option value="Draft">Draft</option>
                  <option value="Confirmed">Confirmed</option>
                  <option value="Delivered">Delivered</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
            </div>
            <div className="modal-footer">
              <button className="cancel-button" onClick={handleCancel}>Cancel</button>
              <button className="save-button" onClick={handleSave}>Save</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
