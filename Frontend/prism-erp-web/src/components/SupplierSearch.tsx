import { useState, useEffect, useRef } from 'react';
import { suppliersApi } from '../services/purchasingApi';
import type { SupplierDto } from '../services/types/purchasing.types';
import SearchModal from './SearchModal';
import './SupplierSearch.css';

interface SupplierSearchProps {
  value?: SupplierDto | null;
  onChange: (supplier: SupplierDto | null) => void;
  disabled?: boolean;
  hasError?: boolean;
}

export default function SupplierSearch({ value, onChange, disabled, hasError }: SupplierSearchProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [suppliers, setSuppliers] = useState<SupplierDto[]>([]);
  const buttonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    const fetchSuppliers = async () => {
      try {
        const data = await suppliersApi.getAll();
        setSuppliers(data);
      } catch (error) {
        console.log('API not available, using mock data');
        setSuppliers([
          { id: 1, code: 'S001', name: 'Food Supplier Co', phone: '0901234567', email: 'food@email.com', address: '123 District 1', taxCode: '123456789', isActive: true },
          { id: 2, code: 'S002', name: 'Beverage Supplier', phone: '0902345678', email: 'beverage@email.com', address: '456 District 2', taxCode: '234567890', isActive: true },
          { id: 3, code: 'S003', name: 'Equipment Supplier', phone: '0903456789', email: 'equipment@email.com', address: '789 District 3', taxCode: '345678901', isActive: true },
        ]);
      }
    };

    fetchSuppliers();
  }, []);

  const handleSelect = (supplier: SupplierDto) => {
    onChange(supplier);
  };

  const handleClear = () => {
    onChange(null);
  };

  // Focus on button when hasError changes to true
  useEffect(() => {
    if (hasError && buttonRef.current && !value) {
      buttonRef.current.focus();
    }
  }, [hasError, value]);

  const columns = [
    { key: 'id' as keyof SupplierDto, label: 'ID', width: '80px' },
    { key: 'code' as keyof SupplierDto, label: 'Code', width: '120px' },
    { key: 'name' as keyof SupplierDto, label: 'Name', width: 'auto' },
    { key: 'phone' as keyof SupplierDto, label: 'Phone', width: '150px' },
  ];

  return (
    <div className="supplier-search">
      <div className="supplier-search-input">
        {value ? (
          <div className="selected-supplier">
            <span className="supplier-code">{value.code}</span>
            <span className="supplier-name">{value.name}</span>
            <span className="supplier-phone">{value.phone || ''}</span>
            {!disabled && (
              <button className="clear-button" onClick={handleClear}>×</button>
            )}
          </div>
        ) : (
          <button
            ref={buttonRef}
            className={`select-supplier-button ${hasError ? 'has-error' : ''}`}
            onClick={() => !disabled && setIsModalOpen(true)}
            disabled={disabled}
          >
            Select Supplier
          </button>
        )}
      </div>

      <SearchModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        data={suppliers}
        columns={columns}
        onSelect={handleSelect}
        title="Select Supplier"
        searchPlaceholder="Search by ID, Code, Name, or Phone..."
        searchableFields={['id', 'code', 'name', 'phone']}
      />
    </div>
  );
}
