import { useState, useEffect, useCallback } from 'react';
import { suppliersApi, purchaseOrdersApi } from '../../../services/purchasingApi';
import { warehousesApi } from '../../../services/inventoryApi';
import type { SupplierDto } from '../../../services/types/purchasing.types';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import type { PurchaseOrder, OrderDetail, OrderLineEditable } from '../types';
import {
  createEmptyLine,
  mapPoLinesFromDto,
  buildMockOrderDetail,
  MOCK_PO_LINES,
} from '../types';
import { calculateOrderTotals } from '../orderCalculations';

interface UsePurchaseOrderFormOptions {
  showToast: (message: string, type: 'success' | 'error') => void;
  refreshOrders: () => Promise<void>;
}

export function usePurchaseOrderForm({ showToast, refreshOrders }: UsePurchaseOrderFormOptions) {
  const [showModal, setShowModal] = useState(false);
  const [orderDetail, setOrderDetail] = useState<OrderDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [editableLines, setEditableLines] = useState<OrderLineEditable[]>([]);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isCreating, setIsCreating] = useState(false);
  const [defaultWarehouseId, setDefaultWarehouseId] = useState(1);
  const [createSupplier, setCreateSupplier] = useState<SupplierDto | null>(null);
  const [createNotes, setCreateNotes] = useState('');
  const [submitting, setSubmitting] = useState<'create' | 'save' | 'approve' | 'cancel' | null>(null);

  useEffect(() => {
    warehousesApi.getAll()
      .then((warehouses) => {
        if (warehouses.length > 0) setDefaultWarehouseId(warehouses[0].id);
      })
      .catch(() => setDefaultWarehouseId(1));
  }, []);

  const refreshOrderDetail = useCallback(async () => {
    await refreshOrders();
    if (!orderDetail) return;
    const data = await purchaseOrdersApi.getById(orderDetail.id);
    const supplierData = await suppliersApi.getById(data.supplierId);
    const lines = mapPoLinesFromDto(data.lines);
    setEditableLines(lines);
    setOrderDetail({
      ...data,
      supplierData,
      supplierName: supplierData.name,
      totalAmount: calculateOrderTotals(lines).totalAmount,
    });
  }, [orderDetail, refreshOrders]);

  const handleAdd = () => {
    setIsCreating(true);
    setOrderDetail(null);
    setCreateSupplier(null);
    setCreateNotes('');
    setEditableLines([createEmptyLine()]);
    setShowModal(true);
  };

  const handleEdit = async (order: PurchaseOrder) => {
    setIsCreating(false);
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const data = await purchaseOrdersApi.getById(order.id);
      const supplierData = await suppliersApi.getById(order.supplierId);
      const lines = mapPoLinesFromDto(data.lines);
      setEditableLines(lines);
      setOrderDetail({
        ...data,
        supplierData,
        supplierName: supplierData.name,
        totalAmount: calculateOrderTotals(lines).totalAmount,
      });
    } catch {
      setEditableLines(MOCK_PO_LINES);
      setOrderDetail(buildMockOrderDetail(order, MOCK_PO_LINES));
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
    setFieldErrors({});
    if (!createSupplier) {
      setFieldErrors({ supplierId: 'Supplier is required' });
      showToast('Please select a supplier', 'error');
      return;
    }
    const validLines = editableLines.filter((line) => line.productId > 0 && line.quantityOrdered > 0);
    if (validLines.length === 0) {
      showToast('Add at least one product line', 'error');
      return;
    }
    try {
      setSubmitting('create');
      await purchaseOrdersApi.create({
        supplierId: createSupplier.id,
        warehouseId: defaultWarehouseId,
        notes: createNotes || undefined,
        lines: validLines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
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
      await purchaseOrdersApi.update(orderDetail.id, {
        supplierId: orderDetail.supplierId,
        warehouseId: orderDetail.warehouseId,
        notes: orderDetail.notes,
        lines: editableLines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
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
          if (fe.field.toLowerCase().includes('supplier')) errors.supplierId = fe.messages[0];
          else errors[fe.field] = fe.messages[0];
        });
        setFieldErrors(errors);
        setTimeout(() => setFieldErrors({}), 2000);
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
      await purchaseOrdersApi.update(orderDetail.id, {
        supplierId: orderDetail.supplierId,
        warehouseId: orderDetail.warehouseId,
        notes: orderDetail.notes,
        lines: editableLines.map((line) => ({
          productId: line.productId,
          quantityOrdered: line.quantityOrdered,
          unitPrice: line.unitPrice,
        })),
      });
      await purchaseOrdersApi.approve(orderDetail.id);
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
      await purchaseOrdersApi.cancel(orderDetail.id);
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
    createSupplier,
    setCreateSupplier,
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
