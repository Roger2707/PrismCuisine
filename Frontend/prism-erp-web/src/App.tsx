import { useState } from 'react';
import Login from './components/Login';
import Sidebar from './components/Sidebar';
import Dashboard from './pages/Dashboard';
import Inventory from './pages/Inventory';
import Purchasing from './pages/Purchasing';
import SalesOrdering from './pages/SalesOrdering';
import InvoiceInquiry from './pages/InvoiceInquiry';
import PaymentInquiry from './pages/PaymentInquiry';
import Customers from './pages/Customers';
import Suppliers from './pages/Suppliers';
import Products from './pages/Products';
import './App.css';
import './styles/status.css';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [currentModule, setCurrentModule] = useState('dashboard');
  const [userEmail, setUserEmail] = useState('');

  const handleLogin = (email: string, _password: string) => {
    setIsAuthenticated(true);
    setUserEmail(email);
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
    setUserEmail('');
    setCurrentModule('dashboard');
  };

  const renderModule = () => {
    switch (currentModule) {
      case 'dashboard':
        return <Dashboard username={userEmail.split('@')[0]} />;
      case 'customers':
        return <Customers />;
      case 'suppliers':
        return <Suppliers />;
      case 'products':
        return <Products />;
      case 'inventory':
        return <Inventory />;
      case 'purchasing':
        return <Purchasing />;
      case 'salesOrdering':
        return <SalesOrdering />;
      case 'invoiceInquiry':
        return <InvoiceInquiry />;
      case 'paymentInquiry':
        return <PaymentInquiry />;
      default:
        return <Dashboard username={userEmail.split('@')[0]} />;
    }
  };

  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} />;
  }

  return (
    <div className="app-container">
      <Sidebar
        currentModule={currentModule}
        onModuleChange={setCurrentModule}
        onLogout={handleLogout}
      />
      <div className="main-content">
        {renderModule()}
      </div>
    </div>
  );
}

export default App;
