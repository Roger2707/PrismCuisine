import { useCallback } from 'react';
import type { ProductDto } from '../../../services/types/inventory.types';
import type { SupplierDto } from '../../../services/types/purchasing.types';
import type { OrderDetail, OrderLineEditable } from '../types';
import { createEmptyLine } from '../types';
import { calculateLineTotals, calculateOrderTotals } from '../orderCalculations';

interface UsePurchaseOrderLinesOptions {
  editableLines: OrderLineEditable[];
  setEditableLines: React.Dispatch<React.SetStateAction<OrderLineEditable[]>>;
  orderDetail: OrderDetail | null;
  setOrderDetail: React.Dispatch<React.SetStateAction<OrderDetail | null>>;
}

export function usePurchaseOrderLines({
  editableLines,
  setEditableLines,
  orderDetail,
  setOrderDetail,
}: UsePurchaseOrderLinesOptions) {
  const updateLinesAndTotals = useCallback(
    (recalculatedLines: OrderLineEditable[]) => {
      setEditableLines(recalculatedLines);
      if (orderDetail) {
        const totals = calculateOrderTotals(recalculatedLines);
        setOrderDetail({ ...orderDetail, totalAmount: totals.totalAmount, lines: recalculatedLines });
      }
    },
    [orderDetail, setEditableLines, setOrderDetail],
  );

  const handleLineChange = useCallback(
    (index: number, field: keyof OrderLineEditable, value: number) => {
      const updatedLines = [...editableLines];
      updatedLines[index] = { ...updatedLines[index], [field]: value };
      updateLinesAndTotals(calculateLineTotals(updatedLines));
    },
    [editableLines, updateLinesAndTotals],
  );

  const handleCreateLineProductChange = useCallback(
    (index: number, product: ProductDto | null) => {
      const updatedLines = [...editableLines];
      updatedLines[index] = {
        ...updatedLines[index],
        productId: product?.id ?? 0,
        productName: product?.name ?? '',
        productData: product,
      };
      setEditableLines(calculateLineTotals(updatedLines));
    },
    [editableLines, setEditableLines],
  );

  const handleAddLine = useCallback(() => {
    updateLinesAndTotals(calculateLineTotals([...editableLines, createEmptyLine()]));
  }, [editableLines, updateLinesAndTotals]);

  const handleRemoveLine = useCallback(
    (index: number) => {
      updateLinesAndTotals(calculateLineTotals(editableLines.filter((_, i) => i !== index)));
    },
    [editableLines, updateLinesAndTotals],
  );

  const handleSupplierChange = useCallback(
    (supplier: SupplierDto | null) => {
      if (!orderDetail) return;
      if (supplier) {
        setOrderDetail({
          ...orderDetail,
          supplierId: supplier.id,
          supplierName: supplier.name,
          supplierData: supplier,
        });
      } else {
        setOrderDetail({ ...orderDetail, supplierId: 0, supplierName: '', supplierData: undefined });
      }
    },
    [orderDetail, setOrderDetail],
  );

  return {
    handleLineChange,
    handleCreateLineProductChange,
    handleAddLine,
    handleRemoveLine,
    handleSupplierChange,
  };
}
