import { useState, useEffect, useCallback } from 'react';
import { purchaseOrdersApi } from '../../../services/purchasingApi';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import type { PurchaseOrder } from '../types';

export function usePurchaseOrderList(showToast: (message: string, type: 'success' | 'error') => void) {
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [loading, setLoading] = useState(true);

  const refreshOrders = useCallback(async () => {
    const data = await purchaseOrdersApi.getAll();
    setOrders(data.map((dto) => ({ ...dto })));
  }, []);

  useEffect(() => {
    refreshOrders()
      .catch(() => setOrders([]))
      .finally(() => setLoading(false));
  }, [refreshOrders]);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to cancel this order?')) return;
    try {
      await purchaseOrdersApi.cancel(id);
      showToast('Order cancelled successfully!', 'success');
      setOrders((prev) => prev.filter((order) => order.id !== id));
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleApproveInline = async (id: number) => {
    try {
      await purchaseOrdersApi.approve(id);
      showToast('Order approved successfully!', 'success');
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  return { orders, loading, refreshOrders, handleDelete, handleApproveInline };
}
