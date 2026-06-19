import { useState, useEffect } from 'react';
import { productsApi } from '../services/inventoryApi';
import type { ProductDto } from '../services/types/inventory.types';
import { ProductInventoryModal } from '../components/inventory/ProductInventoryModal';
import { StatusBadge } from '../utils/statusBadge';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import './Inventory.css';
import './Purchasing.css';

export default function Inventory() {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewProduct, setViewProduct] = useState<ProductDto | null>(null);

  useEffect(() => {
    productsApi.getAll()
      .then(setProducts)
      .catch((err: unknown) => {
        setError(getToastMessage(parseApiError(err)));
        setProducts([]);
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>📦 Inventory Module</h1>
        <p>View warehouse balances, reservations, movements and cost layers</p>
      </div>

      <div className="page-content">
        {error && (
          <p className="error-message" style={{ marginBottom: '16px' }}>{error}</p>
        )}
        <div className="data-table-container">
          <div className="table-header">
            <h2>Product List</h2>
          </div>
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>SKU</th>
                <th>Product Name</th>
                <th>Unit</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id}>
                  <td>{product.id}</td>
                  <td>{product.sku}</td>
                  <td>{product.name}</td>
                  <td>{product.unit}</td>
                  <td><StatusBadge status={product.isActive ? 'Active' : 'Inactive'} /></td>
                  <td>
                    <button className="action-btn edit" onClick={() => setViewProduct(product)}>
                      View Inventory
                    </button>
                  </td>
                </tr>
              ))}
              {products.length === 0 && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center' }}>No products found</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <ProductInventoryModal product={viewProduct} onClose={() => setViewProduct(null)} />
    </div>
  );
}
