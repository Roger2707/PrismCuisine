import { useState, useEffect, useCallback } from 'react';
import { customersApi } from '../services/salesOrderingApi';
import type { CustomerDto, CreateCustomerRequest, UpdateCustomerRequest } from '../services/types/salesOrdering.types';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import './Inventory.css';
import './Purchasing.css';

const emptyForm: CreateCustomerRequest = {
  code: '',
  name: '',
  phone: '',
  email: '',
  address: '',
  taxCode: '',
};

export default function Customers() {
  const [items, setItems] = useState<CustomerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<CustomerDto | null>(null);
  const [form, setForm] = useState<CreateCustomerRequest>(emptyForm);
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  const showToast = useCallback((message: string, type: 'success' | 'error') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), type === 'error' ? 5000 : 3000);
  }, []);

  const loadItems = useCallback(async () => {
    const data = await customersApi.getAll();
    setItems(data);
  }, []);

  useEffect(() => {
    loadItems()
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, [loadItems]);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setShowModal(true);
  };

  const openEdit = (item: CustomerDto) => {
    setEditing(item);
    setForm({
      code: item.code,
      name: item.name,
      phone: item.phone || '',
      email: item.email || '',
      address: item.address || '',
      taxCode: item.taxCode || '',
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    try {
      if (editing) {
        const update: UpdateCustomerRequest = {
          name: form.name,
          phone: form.phone || undefined,
          email: form.email || undefined,
          address: form.address || undefined,
          taxCode: form.taxCode || undefined,
        };
        await customersApi.update(editing.id, update);
        showToast('Customer updated successfully!', 'success');
      } else {
        await customersApi.create(form);
        showToast('Customer created successfully!', 'success');
      }
      setShowModal(false);
      await loadItems();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleDeactivate = async (id: number) => {
    if (!window.confirm('Deactivate this customer?')) return;
    try {
      await customersApi.deactivate(id);
      showToast('Customer deactivated!', 'success');
      await loadItems();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>👥 Customers</h1>
        <p>Manage customer master data</p>
      </div>
      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>Customer List</h2>
            <button className="add-button" onClick={openCreate}>+ Add Customer</button>
          </div>
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Code</th>
                <th>Name</th>
                <th>Phone</th>
                <th>Email</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td>{item.id}</td>
                  <td>{item.code}</td>
                  <td>{item.name}</td>
                  <td>{item.phone || '-'}</td>
                  <td>{item.email || '-'}</td>
                  <td>
                    <span className={`status ${item.isActive ? 'active' : 'cancelled'}`}>
                      {item.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>
                    <button className="action-btn edit" onClick={() => openEdit(item)}>Edit</button>
                    {item.isActive && (
                      <button className="action-btn delete" onClick={() => handleDeactivate(item.id)}>Deactivate</button>
                    )}
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
              <h2>{editing ? 'Edit Customer' : 'Create Customer'}</h2>
              <button className="close-button" onClick={() => setShowModal(false)}>×</button>
            </div>
            <div className="modal-body">
              {!editing && (
                <div className="form-group">
                  <label>Code</label>
                  <input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} />
                </div>
              )}
              <div className="form-group">
                <label>Name</label>
                <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Phone</label>
                <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Address</label>
                <textarea value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} rows={2} />
              </div>
              <div className="form-group">
                <label>Tax Code</label>
                <input value={form.taxCode} onChange={(e) => setForm({ ...form, taxCode: e.target.value })} />
              </div>
            </div>
            <div className="modal-footer">
              <button className="cancel-button" onClick={() => setShowModal(false)}>Cancel</button>
              <button className="save-button" onClick={handleSave}>Save</button>
            </div>
          </div>
        </div>
      )}

      {toast && <div className={`toast toast-${toast.type}`}>{toast.message}</div>}
    </div>
  );
}
