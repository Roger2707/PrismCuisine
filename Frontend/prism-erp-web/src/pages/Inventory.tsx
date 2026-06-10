import { useState, useEffect } from 'react';
import { productsApi } from '../services/api';
import './Inventory.css';

interface Product {
  id: number;
  sku: string;
  name: string;
  unit: string;
  quantity: number;
  cost: number;
  category: string;
  balance: number;
}

export default function Inventory() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await productsApi.getAll();
        setProducts(data);
      } catch (error) {
        console.log('API not available, using mock data');
        // Fallback to mock data
        setProducts([
          { id: 1, sku: 'P001', name: 'Water Spinach', unit: 'KG', quantity: 20, cost: 15000, category: 'Ingredients', balance: 15 },
          { id: 2, sku: 'P002', name: 'Pork Belly', unit: 'KG', quantity: 10, cost: 120000, category: 'Ingredients', balance: 8 },
          { id: 3, sku: 'P003', name: 'Black Tiger Shrimp', unit: 'KG', quantity: 5, cost: 350000, category: 'Ingredients', balance: 5 },
          { id: 4, sku: 'P004', name: 'Coca Cola', unit: 'CASE', quantity: 12, cost: 180000, category: 'Beverages', balance: 10 },
          { id: 5, sku: 'P005', name: 'Phu Quoc Fish Sauce', unit: 'BOTTLE', quantity: 6, cost: 45000, category: 'Seasonings', balance: 4 },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>📦 Inventory Module</h1>
        <p>Manage warehouse and inventory</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>Product List</h2>
            <button className="add-button">+ Add Product</button>
          </div>
          
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>SKU</th>
                <th>Product Name</th>
                <th>Unit</th>
                <th>Quantity</th>
                <th>Balance</th>
                <th>Cost</th>
                <th>Category</th>
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
                  <td>{product.quantity}</td>
                  <td>{product.balance}</td>
                  <td>₫{product.cost.toLocaleString()}</td>
                  <td>{product.category}</td>
                  <td>
                    <button className="action-btn edit">Edit</button>
                    <button className="action-btn delete">Delete</button>
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
