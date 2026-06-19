import { useState } from 'react';
import Login from './components/Login';
import { AuthInitializer } from './components/AuthInitializer';
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
import { useAppDispatch, useAppSelector } from './app/hooks';
import { clearAuth } from './app/userSlice';
import { authApi } from './services/identityApi';
import './App.css';
import './styles/status.css';

function AppContent() {
  const dispatch = useAppDispatch();
  const isAuthenticated = useAppSelector((state) => state.user.isAuthenticated);
  const user = useAppSelector((state) => state.user.user);
  const [currentModule, setCurrentModule] = useState('dashboard');

  const handleLogout = async () => {
    try {
      await authApi.logout({ refreshToken: '' });
    } catch {
      // Refresh token is in HttpOnly cookie; client-side session is cleared regardless.
    }
    dispatch(clearAuth());
    setCurrentModule('dashboard');
  };

  const renderModule = () => {
    const username = user?.displayName ?? user?.email?.split('@')[0] ?? 'User';

    switch (currentModule) {
      case 'dashboard':
        return <Dashboard username={username} />;
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
        return <Dashboard username={username} />;
    }
  };

  if (!isAuthenticated) {
    return <Login />;
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

function App() {
  return (
    <AuthInitializer>
      <AppContent />
    </AuthInitializer>
  );
}

export default App;
