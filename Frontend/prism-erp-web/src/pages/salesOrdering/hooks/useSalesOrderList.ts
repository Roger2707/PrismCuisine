import { useState, useEffect, useCallback } from 'react';
import { salesOrdersApi } from '../../../services/salesOrderingApi';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import type { SalesOrder } from '../types';

const MOCK_ORDERS: SalesOrder[] = [
  {
    id: 1,
    orderNumber: 'SO-2024-001',
    totalAmount: 2500000,
    status: 'Draft',
    invoiceStatus: 'NotInvoiced',
    orderDate: '2024-01-15',
    customerId: 1,
    customerName: 'Sen Vang Restaurant',
    subTotal: 0,
    totalDiscount: 0,
    totalVAT: 0,
  },
];

export function useSalesOrderList(showToast: (message: string, type: 'success' | 'error') => void) {
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [loading, setLoading] = useState(true);

  const refreshOrders = useCallback(async () => {
    const data = await salesOrdersApi.getAll();
    setOrders(data.map((dto) => ({ ...dto })));
  }, []);

  useEffect(() => {
    refreshOrders()
      .catch(() => setOrders(MOCK_ORDERS))
      .finally(() => setLoading(false));
  }, [refreshOrders]);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to cancel this order?')) return;
    try {
      await salesOrdersApi.cancel(id);
      showToast('Order cancelled successfully!', 'success');
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleApproveInline = async (id: number) => {
    try {
      await salesOrdersApi.approve(id);
      showToast('Order approved successfully!', 'success');
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  return { orders, loading, refreshOrders, handleDelete, handleApproveInline };
}
