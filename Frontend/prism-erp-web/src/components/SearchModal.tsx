import { useState } from 'react';
import './SearchModal.css';

interface Column<T> {
  key: keyof T;
  label: string;
  width?: string;
}

interface SearchModalProps<T> {
  isOpen: boolean;
  onClose: () => void;
  data: T[];
  columns: Column<T>[];
  onSelect: (item: T) => void;
  title?: string;
  searchPlaceholder?: string;
  searchableFields?: (keyof T)[];
}

export default function SearchModal<T extends Record<string, any>>({
  isOpen,
  onClose,
  data,
  columns,
  onSelect,
  title = 'Search',
  searchPlaceholder = 'Search...',
  searchableFields,
}: SearchModalProps<T>) {
  const [searchTerm, setSearchTerm] = useState('');

  if (!isOpen) return null;

  const filteredData = data.filter(item => {
    if (!searchTerm) return true;
    const fieldsToSearch = searchableFields || columns.map(col => col.key);
    return fieldsToSearch.some(field => {
      const value = item[field];
      if (value === null || value === undefined) return false;
      return String(value).toLowerCase().includes(searchTerm.toLowerCase());
    });
  });

  const handleSelect = (item: T) => {
    onSelect(item);
    onClose();
    setSearchTerm('');
  };

  return (
    <div className="search-modal-overlay" onClick={onClose}>
      <div className="search-modal" onClick={(e) => e.stopPropagation()}>
        <div className="search-modal-header">
          <h3>{title}</h3>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="search-modal-body">
          <div className="search-input-container">
            <input
              type="text"
              placeholder={searchPlaceholder}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
              autoFocus
            />
          </div>
          <div className="search-results-container">
            {filteredData.length === 0 ? (
              <div className="no-results">No results found</div>
            ) : (
              <table className="search-table">
                <thead>
                  <tr>
                    {columns.map((col) => (
                      <th key={String(col.key)} style={{ width: col.width }}>
                        {col.label}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {filteredData.map((item, index) => (
                    <tr
                      key={index}
                      className="search-row"
                      onClick={() => handleSelect(item)}
                    >
                      {columns.map((col) => (
                        <td key={String(col.key)}>{String(item[col.key] || '')}</td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
        <div className="search-modal-footer">
          <button className="cancel-button" onClick={onClose}>Cancel</button>
        </div>
      </div>
    </div>
  );
}
