import { useState, useEffect } from 'react';
import { productsApi } from '../services/inventoryApi';
import type { ProductDto } from '../services/types/inventory.types';
import './Inventory.css';

interface Product extends ProductDto {
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
        // Map ProductDto to Product interface with additional fields
        const mappedProducts: Product[] = data.map(dto => ({
          ...dto,
          quantity: 0, // Will be populated from balance API
          cost: 0, // Will be populated from balance API
          category: 'Ingredients', // Will be populated from category API
          balance: 0, // Will be populated from balance API
        }));
        setProducts(mappedProducts);
      } catch (error) {
        console.log('API not available, using mock data');
        // Fallback to mock data
        setProducts([
          { id: 1, sku: 'P001', name: 'Water Spinach', unit: 'KG', quantity: 20, cost: 15000, category: 'Ingredients', balance: 15, categoryId: 1, isActive: true },
          { id: 2, sku: 'P002', name: 'Pork Belly', unit: 'KG', quantity: 10, cost: 120000, category: 'Ingredients', balance: 8, categoryId: 1, isActive: true },
          { id: 3, sku: 'P003', name: 'Black Tiger Shrimp', unit: 'KG', quantity: 5, cost: 350000, category: 'Ingredients', balance: 5, categoryId: 1, isActive: true },
          { id: 4, sku: 'P004', name: 'Coca Cola', unit: 'CASE', quantity: 12, cost: 180000, category: 'Beverages', balance: 10, categoryId: 2, isActive: true },
          { id: 5, sku: 'P005', name: 'Phu Quoc Fish Sauce', unit: 'BOTTLE', quantity: 6, cost: 45000, category: 'Seasonings', balance: 4, categoryId: 3, isActive: true },
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
