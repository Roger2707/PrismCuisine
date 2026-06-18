import { useState, useEffect } from 'react';
import { productsApi } from '../services/inventoryApi';
import type { ProductDto } from '../services/types/inventory.types';
import { ProductInventoryModal } from '../components/inventory/ProductInventoryModal';
import { StatusBadge } from '../utils/statusBadge';
import './Inventory.css';
import './Purchasing.css';

export default function Inventory() {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [viewProduct, setViewProduct] = useState<ProductDto | null>(null);

  useEffect(() => {
    productsApi.getAll()
      .then(setProducts)
      .catch(() => {
        setProducts([
          { id: 1, sku: 'P001', name: 'Water Spinach', unit: 'KG', categoryId: 1, isActive: true },
          { id: 2, sku: 'P002', name: 'Pork Belly', unit: 'KG', categoryId: 1, isActive: true },
        ]);
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
            </tbody>
          </table>
        </div>
      </div>

      <ProductInventoryModal product={viewProduct} onClose={() => setViewProduct(null)} />
    </div>
  );
}
