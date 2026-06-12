import { useState, useEffect, useCallback } from 'react';
import { productsApi, productCategoriesApi } from '../services/inventoryApi';
import type { ProductDto, ProductCategoryDto, CreateProductRequest, UpdateProductRequest } from '../services/types/inventory.types';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import './Inventory.css';
import './Purchasing.css';

export default function Products() {
  const [items, setItems] = useState<ProductDto[]>([]);
  const [categories, setCategories] = useState<ProductCategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<ProductDto | null>(null);
  const [form, setForm] = useState<CreateProductRequest>({
    categoryId: 1,
    sku: '',
    name: '',
    unit: 'KG',
    description: '',
  });
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  const showToast = useCallback((message: string, type: 'success' | 'error') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), type === 'error' ? 5000 : 3000);
  }, []);

  const loadItems = useCallback(async () => {
    const [products, cats] = await Promise.all([
      productsApi.getAll(),
      productCategoriesApi.getAll().catch(() => []),
    ]);
    setItems(products);
    setCategories(cats);
    if (cats.length > 0) {
      setForm((prev) => ({ ...prev, categoryId: prev.categoryId || cats[0].id }));
    }
  }, []);

  useEffect(() => {
    loadItems()
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, [loadItems]);

  const categoryName = (id: number) => categories.find((c) => c.id === id)?.name || id;

  const openCreate = () => {
    setEditing(null);
    setForm({
      categoryId: categories[0]?.id || 1,
      sku: '',
      name: '',
      unit: 'KG',
      description: '',
    });
    setShowModal(true);
  };

  const openEdit = (item: ProductDto) => {
    setEditing(item);
    setForm({
      categoryId: item.categoryId,
      sku: item.sku,
      name: item.name,
      unit: item.unit,
      description: item.description || '',
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    try {
      if (editing) {
        const update: UpdateProductRequest = {
          categoryId: form.categoryId,
          name: form.name,
          unit: form.unit,
          description: form.description || undefined,
        };
        await productsApi.update(editing.id, update);
        showToast('Product updated successfully!', 'success');
      } else {
        await productsApi.create(form);
        showToast('Product created successfully!', 'success');
      }
      setShowModal(false);
      await loadItems();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleDeactivate = async (id: number) => {
    if (!window.confirm('Deactivate this product?')) return;
    try {
      await productsApi.deactivate(id);
      showToast('Product deactivated!', 'success');
      await loadItems();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>📦 Products</h1>
        <p>Manage product master data</p>
      </div>
      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>Product List</h2>
            <button className="add-button" onClick={openCreate}>+ Add Product</button>
          </div>
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>SKU</th>
                <th>Name</th>
                <th>Unit</th>
                <th>Category</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td>{item.id}</td>
                  <td>{item.sku}</td>
                  <td>{item.name}</td>
                  <td>{item.unit}</td>
                  <td>{categoryName(item.categoryId)}</td>
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
              <h2>{editing ? 'Edit Product' : 'Create Product'}</h2>
              <button className="close-button" onClick={() => setShowModal(false)}>×</button>
            </div>
            <div className="modal-body">
              {!editing && (
                <div className="form-group">
                  <label>SKU</label>
                  <input value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} />
                </div>
              )}
              <div className="form-group">
                <label>Name</label>
                <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Unit</label>
                <input value={form.unit} onChange={(e) => setForm({ ...form, unit: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Category</label>
                <select
                  value={form.categoryId}
                  onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })}
                >
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                  ))}
                  {categories.length === 0 && <option value={1}>Default</option>}
                </select>
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} rows={2} />
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
