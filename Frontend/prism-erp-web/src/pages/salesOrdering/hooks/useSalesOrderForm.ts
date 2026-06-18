import { useState, useCallback } from 'react';
import { customersApi, salesOrdersApi } from '../../../services/salesOrderingApi';
import type { CustomerDto } from '../../../services/types/salesOrdering.types';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import type { SalesOrder, OrderDetail, OrderLineEditable } from '../types';
import { createEmptyLine } from '../types';

interface UseSalesOrderFormOptions {
  showToast: (message: string, type: 'success' | 'error') => void;
  refreshOrders: () => Promise<void>;
}

export function useSalesOrderForm({ showToast, refreshOrders }: UseSalesOrderFormOptions) {
  const [showModal, setShowModal] = useState(false);
  const [orderDetail, setOrderDetail] = useState<OrderDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [editableLines, setEditableLines] = useState<OrderLineEditable[]>([]);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isCreating, setIsCreating] = useState(false);
  const [createCustomer, setCreateCustomer] = useState<CustomerDto | null>(null);
  const [createNotes, setCreateNotes] = useState('');
  const [submitting, setSubmitting] = useState<'create' | 'save' | 'approve' | 'cancel' | null>(null);

  const refreshOrderDetail = useCallback(async () => {
    await refreshOrders();
    if (!orderDetail) return;
    const data = await salesOrdersApi.getById(orderDetail.id);
    const customerData = await customersApi.getById(data.customerId);
    const lines = data.lines.map((line) => ({ ...line }));
    setEditableLines(lines);
    setOrderDetail({ ...data, customerData });
  }, [orderDetail, refreshOrders]);

  const handleAdd = () => {
    setIsCreating(true);
    setOrderDetail(null);
    setCreateCustomer(null);
    setCreateNotes('');
    setEditableLines([createEmptyLine()]);
    setShowModal(true);
  };

  const handleEdit = async (order: SalesOrder) => {
    setIsCreating(false);
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const data = await salesOrdersApi.getById(order.id);
      const customerData = await customersApi.getById(order.customerId);
      setEditableLines(data.lines.map((line) => ({ ...line })));
      setOrderDetail({ ...data, customerData });
    } catch {
      setEditableLines([]);
      setOrderDetail(null);
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleCancel = () => {
    setShowModal(false);
    setOrderDetail(null);
    setEditableLines([]);
    setFieldErrors({});
    setIsCreating(false);
  };

  const handleCreateSave = async () => {
    if (!createCustomer) {
      setFieldErrors({ customerId: 'Customer is required' });
      showToast('Please select a customer', 'error');
      return;
    }
    const validLines = editableLines.filter((line) => line.productId > 0 && line.quantityOrdered > 0);
    if (validLines.length === 0) {
      showToast('Add at least one product line', 'error');
      return;
    }
    try {
      setSubmitting('create');
      await salesOrdersApi.create({
        customerId: createCustomer.id,
        customerName: createCustomer.name,
        notes: createNotes || undefined,
        lines: validLines.map((line) => ({
          productId: line.productId,
          productName: line.productName,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
          discountPercent: line.discountPercent,
          vatRate: line.vatRate,
        })),
      });
      showToast('Order created successfully!', 'success');
      setShowModal(false);
      setIsCreating(false);
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setSubmitting(null);
    }
  };

  const handleSave = async () => {
    if (!orderDetail) return;
    setFieldErrors({});
    try {
      setSubmitting('save');
      await salesOrdersApi.update(orderDetail.id, {
        customerId: orderDetail.customerId,
        customerName: orderDetail.customerName,
        notes: orderDetail.notes,
        lines: editableLines.map((line) => ({
          productId: line.productId,
          productName: line.productName,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
          discountPercent: line.discountPercent,
          vatRate: line.vatRate,
        })),
      });
      showToast('Order updated successfully!', 'success');
      setShowModal(false);
      await refreshOrders();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      if (apiError.type === 'validation-error' && apiError.fieldErrors) {
        const errors: Record<string, string> = {};
        apiError.fieldErrors.forEach((fe) => {
          if (fe.field.toLowerCase().includes('customer')) errors.customerId = fe.messages[0];
          else errors[fe.field] = fe.messages[0];
        });
        setFieldErrors(errors);
      }
      showToast(getToastMessage(apiError), 'error');
    } finally {
      setSubmitting(null);
    }
  };

  const handleApprove = async () => {
    if (!orderDetail) return;
    try {
      setSubmitting('approve');
      await salesOrdersApi.update(orderDetail.id, {
        customerId: orderDetail.customerId,
        customerName: orderDetail.customerName,
        notes: orderDetail.notes,
        lines: editableLines.map((line) => ({
          productId: line.productId,
          productName: line.productName,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
          discountPercent: line.discountPercent,
          vatRate: line.vatRate,
        })),
      });
      await salesOrdersApi.approve(orderDetail.id);
      showToast('Order approved successfully!', 'success');
      setShowModal(false);
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setSubmitting(null);
    }
  };

  const handleCancelOrder = async () => {
    if (!orderDetail) return;
    if (!window.confirm('Are you sure you want to cancel this order?')) return;
    try {
      setSubmitting('cancel');
      await salesOrdersApi.cancel(orderDetail.id);
      showToast('Order cancelled successfully!', 'success');
      setShowModal(false);
      await refreshOrders();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setSubmitting(null);
    }
  };

  return {
    showModal,
    orderDetail,
    setOrderDetail,
    loadingDetail,
    editableLines,
    setEditableLines,
    fieldErrors,
    isCreating,
    createCustomer,
    setCreateCustomer,
    createNotes,
    setCreateNotes,
    refreshOrderDetail,
    handleAdd,
    handleEdit,
    handleCancel,
    handleCreateSave,
    handleSave,
    handleApprove,
    handleCancelOrder,
    submitting,
  };
}
