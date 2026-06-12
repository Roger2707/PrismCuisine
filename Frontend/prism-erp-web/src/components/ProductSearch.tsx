import { useState, useEffect } from 'react';
import { productsApi } from '../services/inventoryApi';
import type { ProductDto } from '../services/types/inventory.types';
import SearchModal from './SearchModal';
import './SupplierSearch.css';

interface ProductSearchProps {
  value?: ProductDto | null;
  onChange: (product: ProductDto | null) => void;
  disabled?: boolean;
}

export default function ProductSearch({ value, onChange, disabled }: ProductSearchProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [products, setProducts] = useState<ProductDto[]>([]);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await productsApi.getAll();
        setProducts(data.filter((p) => p.isActive));
      } catch {
        setProducts([
          { id: 1, categoryId: 1, sku: 'P001', name: 'Water Spinach', unit: 'KG', isActive: true },
          { id: 2, categoryId: 1, sku: 'P002', name: 'Pork Belly', unit: 'KG', isActive: true },
          { id: 3, categoryId: 1, sku: 'P003', name: 'Black Tiger Shrimp', unit: 'KG', isActive: true },
        ]);
      }
    };
    fetchProducts();
  }, []);

  const columns = [
    { key: 'id' as keyof ProductDto, label: 'ID', width: '80px' },
    { key: 'sku' as keyof ProductDto, label: 'SKU', width: '120px' },
    { key: 'name' as keyof ProductDto, label: 'Name', width: 'auto' },
    { key: 'unit' as keyof ProductDto, label: 'Unit', width: '80px' },
  ];

  return (
    <div className="supplier-search">
      <div className="supplier-search-input">
        {value ? (
          <div className="selected-supplier">
            <span className="supplier-code">{value.sku}</span>
            <span className="supplier-name">{value.name}</span>
            {!disabled && (
              <button className="clear-button" onClick={() => onChange(null)}>×</button>
            )}
          </div>
        ) : (
          <button
            className="select-supplier-button"
            onClick={() => !disabled && setIsModalOpen(true)}
            disabled={disabled}
          >
            Select Product
          </button>
        )}
      </div>
      <SearchModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        data={products}
        columns={columns}
        onSelect={onChange}
        title="Select Product"
        searchPlaceholder="Search by ID, SKU, or Name..."
        searchableFields={['id', 'sku', 'name']}
      />
    </div>
  );
}
