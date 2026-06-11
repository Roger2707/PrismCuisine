import { useState, useEffect, useRef } from 'react';
import { customersApi } from '../services/salesOrderingApi';
import type { CustomerDto } from '../services/types/salesOrdering.types';
import SearchModal from './SearchModal';
import './CustomerSearch.css';

interface CustomerSearchProps {
  value?: CustomerDto | null;
  onChange: (customer: CustomerDto | null) => void;
  disabled?: boolean;
  hasError?: boolean;
}

export default function CustomerSearch({ value, onChange, disabled, hasError }: CustomerSearchProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const buttonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    const fetchCustomers = async () => {
      try {
        const data = await customersApi.getAll();
        console.log('aaaaa: ' + data);
        setCustomers(data);
      } catch (error) {
        console.log('API not available, using mock data');
        setCustomers([
          { id: 1, code: 'C001', name: 'Sen Vang Restaurant', phone: '0901234567', email: 'senvang@email.com', address: '123 District 1', taxCode: '123456789', isActive: true },
          { id: 2, code: 'C002', name: 'Com Nieu Restaurant', phone: '0902345678', email: 'comnieu@email.com', address: '456 District 2', taxCode: '234567890', isActive: true },
          { id: 3, code: 'C003', name: 'Riverside Hotel', phone: '0903456789', email: 'riverside@email.com', address: '789 District 3', taxCode: '345678901', isActive: true },
        ]);
      }
    };

    fetchCustomers();
  }, []);

  const handleSelect = (customer: CustomerDto) => {
    onChange(customer);
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
    { key: 'id' as keyof CustomerDto, label: 'ID', width: '80px' },
    { key: 'code' as keyof CustomerDto, label: 'Code', width: '120px' },
    { key: 'name' as keyof CustomerDto, label: 'Name', width: 'auto' },
    { key: 'phone' as keyof CustomerDto, label: 'Phone', width: '150px' },
  ];

  return (
    <div className="customer-search">
      <div className="customer-search-input">
        {value ? (
          <div className="selected-customer">
            <span className="customer-code">{value.code}</span>
            <span className="customer-name">{value.name}</span>
            <span className="customer-phone">{value.phone || ''}</span>
            {!disabled && (
              <button className="clear-button" onClick={handleClear}>×</button>
            )}
          </div>
        ) : (
          <button
            ref={buttonRef}
            className={`select-customer-button ${hasError ? 'has-error' : ''}`}
            onClick={() => !disabled && setIsModalOpen(true)}
            disabled={disabled}
          >
            Select Customer
          </button>
        )}
      </div>

      <SearchModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        data={customers}
        columns={columns}
        onSelect={handleSelect}
        title="Select Customer"
        searchPlaceholder="Search by ID, Code, Name, or Phone..."
        searchableFields={['id', 'code', 'name', 'phone']}
      />
    </div>
  );
}
