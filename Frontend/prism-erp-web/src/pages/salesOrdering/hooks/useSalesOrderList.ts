import { useState, useEffect, useCallback } from 'react';
import { salesOrdersApi } from '../../../services/salesOrderingApi';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import { confirmAction, ConfirmMessages } from '../../../utils/confirmAction';
import type { SalesOrder } from '../types';

export function useSalesOrderList(showToast: (message: string, type: 'success' | 'error') => void) {
  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [loading, setLoading] = useState(true);

  const refreshOrders = useCallback(async () => {
    const data = await salesOrdersApi.getAll();
    setOrders(data.map((dto) => ({ ...dto })));
  }, []);

  useEffect(() => {
    refreshOrders()
      .catch((error: unknown) => {
        showToast(getToastMessage(parseApiError(error)), 'error');
        setOrders([]);
      })
      .finally(() => setLoading(false));
  }, [refreshOrders, showToast]);

  const handleDelete = async (id: number) => {
    if (!confirmAction(ConfirmMessages.cancelSalesOrder)) return;
    try {
      await salesOrdersApi.cancel(id);
      showToast('Order cancelled successfully!', 'success');
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  };

  const handleApproveInline = async (id: number) => {
    if (!confirmAction(ConfirmMessages.approveSalesOrder)) return;
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
