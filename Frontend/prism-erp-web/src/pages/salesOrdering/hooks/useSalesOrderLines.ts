import { useCallback } from 'react';
import type { ProductDto } from '../../../services/types/inventory.types';
import type { CustomerDto } from '../../../services/types/salesOrdering.types';
import type { OrderDetail, OrderLineEditable } from '../types';
import { createEmptyLine } from '../types';
import { calculateLineTotals, calculateOrderTotals } from '../orderCalculations';

interface UseSalesOrderLinesOptions {
  editableLines: OrderLineEditable[];
  setEditableLines: React.Dispatch<React.SetStateAction<OrderLineEditable[]>>;
  orderDetail: OrderDetail | null;
  setOrderDetail: React.Dispatch<React.SetStateAction<OrderDetail | null>>;
}

export function useSalesOrderLines({
  editableLines,
  setEditableLines,
  orderDetail,
  setOrderDetail,
}: UseSalesOrderLinesOptions) {
  const handleLineChange = useCallback(
    (index: number, field: keyof OrderLineEditable, value: number) => {
      const updatedLines = [...editableLines];
      updatedLines[index] = { ...updatedLines[index], [field]: value };
      const recalculatedLines = calculateLineTotals(updatedLines);
      setEditableLines(recalculatedLines);
      if (orderDetail) {
        const totals = calculateOrderTotals(recalculatedLines);
        setOrderDetail({ ...orderDetail, ...totals, lines: recalculatedLines });
      }
    },
    [editableLines, orderDetail, setEditableLines, setOrderDetail],
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
    setEditableLines(calculateLineTotals([...editableLines, createEmptyLine()]));
  }, [editableLines, setEditableLines]);

  const handleRemoveLine = useCallback(
    (index: number) => {
      setEditableLines(calculateLineTotals(editableLines.filter((_, i) => i !== index)));
    },
    [editableLines, setEditableLines],
  );

  const handleCustomerChange = useCallback(
    (customer: CustomerDto | null) => {
      if (!orderDetail) return;
      if (customer) {
        setOrderDetail({
          ...orderDetail,
          customerId: customer.id,
          customerName: customer.name,
          customerData: customer,
        });
      } else {
        setOrderDetail({ ...orderDetail, customerId: 0, customerName: '', customerData: undefined });
      }
    },
    [orderDetail, setOrderDetail],
  );

  return {
    handleLineChange,
    handleCreateLineProductChange,
    handleAddLine,
    handleRemoveLine,
    handleCustomerChange,
  };
}
