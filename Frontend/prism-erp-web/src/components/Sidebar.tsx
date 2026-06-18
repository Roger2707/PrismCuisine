import { useState } from 'react';
import './Sidebar.css';

interface SidebarProps {
  currentModule: string;
  onModuleChange: (module: string) => void;
  onLogout: () => void;
}

export default function Sidebar({ currentModule, onModuleChange, onLogout }: SidebarProps) {
  const [isCollapsed, setIsCollapsed] = useState(false);

  const modules = [
    { id: 'dashboard', name: 'Dashboard', icon: '📊' },
    { id: 'customers', name: 'Customer', icon: '👥' },
    { id: 'suppliers', name: 'Supplier', icon: '🏭' },
    { id: 'products', name: 'Product', icon: '📦' },
    { id: 'purchasing', name: 'Purchase Order', icon: '🛒' },
    { id: 'salesOrdering', name: 'Sales Order', icon: '📋' },
    { id: 'invoiceInquiry', name: 'Invoice Inquiry', icon: '🧾' },
    { id: 'paymentInquiry', name: 'Payment Inquiry', icon: '💳' },
    { id: 'inventory', name: 'Inventory', icon: '🗄️' },
  ];

  return (
    <aside className={`sidebar ${isCollapsed ? 'collapsed' : ''}`}>
      <div className="sidebar-header">
        <div className="sidebar-logo">
          <span className="logo-icon">💎</span>
          {!isCollapsed && <span className="logo-text">Prism ERP</span>}
        </div>
        <button
          className="collapse-button"
          onClick={() => setIsCollapsed(!isCollapsed)}
          aria-label="Toggle sidebar"
        >
          {isCollapsed ? '→' : '←'}
        </button>
      </div>

      <nav className="sidebar-nav">
        <ul className="nav-list">
          {modules.map((module) => (
            <li key={module.id} className="nav-item">
              <button
                className={`nav-button ${currentModule === module.id ? 'active' : ''}`}
                onClick={() => onModuleChange(module.id)}
                title={module.name}
              >
                <span className="nav-icon">{module.icon}</span>
                {!isCollapsed && <span className="nav-text">{module.name}</span>}
              </button>
            </li>
          ))}
        </ul>
      </nav>

      <div className="sidebar-footer">
        <button className="logout-button" onClick={onLogout} title="Logout">
          <span className="logout-icon">🚪</span>
          {!isCollapsed && <span className="logout-text">Logout</span>}
        </button>
      </div>
    </aside>
  );
}
