import { useState, useCallback } from 'react';
import { goodsReceiptsApi } from '../../services/purchasingApi';
import type {
  PurchaseOrderDto,
  PurchaseOrderLineDto,
  GoodsReceiptDto,
  GoodsReceiptSummaryDto,
} from '../../services/types/purchasing.types';
import { parseApiError, getToastMessage } from '../../utils/errorHandler';

export interface GoodsReceiptLineEditable {
  id: number;
  purchaseOrderLineId: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityRemaining: number;
  quantityReceived: number;
  unitCost: number;
  lineTotal: number;
}

export function normalizeStatus(status: string): string {
  return status.toLowerCase().replace(/[^a-z]/g, '');
}

export function isPoDraftOrCancelled(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'draft' || s === 'cancelled';
}

export function isPoApproved(status: string): boolean {
  return normalizeStatus(status) === 'approved';
}

export function isPoPartiallyReceived(status: string): boolean {
  return normalizeStatus(status) === 'partiallyreceived';
}

export function isPoFullyReceived(status: string): boolean {
  return normalizeStatus(status) === 'received';
}

export function canOpenGoodsReceipt(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'approved' || s === 'partiallyreceived' || s === 'received';
}

export function isGoodsReceiptPosted(status: string): boolean {
  return normalizeStatus(status) === 'posted';
}

export function isGoodsReceiptDraft(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

function mapPoLineToGrLine(
  line: PurchaseOrderLineDto,
  quantityReceived: number,
  lineId = 0,
): GoodsReceiptLineEditable {
  return {
    id: lineId,
    purchaseOrderLineId: line.id,
    productId: line.productId,
    productName: `Product ${line.productId}`,
    quantityOrdered: line.quantityOrdered,
    quantityRemaining: line.quantityRemaining,
    quantityReceived,
    unitCost: line.unitPrice,
    lineTotal: quantityReceived * line.unitPrice,
  };
}

function mapReceiptLineToEditable(
  receiptLine: GoodsReceiptDto['lines'][number],
  poLine?: PurchaseOrderLineDto,
): GoodsReceiptLineEditable {
  return {
    id: receiptLine.id,
    purchaseOrderLineId: receiptLine.purchaseOrderLineId,
    productId: receiptLine.productId,
    productName: `Product ${receiptLine.productId}`,
    quantityOrdered: poLine?.quantityOrdered ?? 0,
    quantityRemaining: poLine?.quantityRemaining ?? 0,
    quantityReceived: receiptLine.quantity,
    unitCost: receiptLine.unitCost,
    lineTotal: receiptLine.quantity * receiptLine.unitCost,
  };
}

function buildNewDraftReceipt(purchaseOrderId: number, suffix = ''): GoodsReceiptDto {
  const base = `GR-${new Date().getFullYear()}-${String(purchaseOrderId).padStart(4, '0')}`;
  return {
    id: 0,
    receiptNumber: suffix ? `${base}-${suffix}` : base,
    purchaseOrderId,
    status: 'Draft',
    postedAt: undefined,
    notes: '',
    lines: [],
  };
}

function buildLinesForSave(lines: GoodsReceiptLineEditable[]) {
  return lines
    .filter((line) => line.quantityReceived > 0)
    .map((line) => ({
      purchaseOrderLineId: line.purchaseOrderLineId,
      quantity: line.quantityReceived,
      unitCost: line.unitCost,
    }));
}

interface UseGoodsReceiptFromPOOptions {
  showToast: (message: string, type: 'success' | 'error') => void;
  onPurchaseOrderChanged?: () => Promise<void>;
}

export function useGoodsReceiptFromPO({ showToast, onPurchaseOrderChanged }: UseGoodsReceiptFromPOOptions) {
  const [showEditModal, setShowEditModal] = useState(false);
  const [showSearchModal, setShowSearchModal] = useState(false);
  const [goodsReceipt, setGoodsReceipt] = useState<GoodsReceiptDto | null>(null);
  const [goodsReceiptLines, setGoodsReceiptLines] = useState<GoodsReceiptLineEditable[]>([]);
  const [loading, setLoading] = useState(false);
  const [goodsReceiptList, setGoodsReceiptList] = useState<GoodsReceiptSummaryDto[]>([]);
  const [activePurchaseOrder, setActivePurchaseOrder] = useState<PurchaseOrderDto | null>(null);

  const closeEditModal = useCallback(() => {
    setShowEditModal(false);
    setGoodsReceipt(null);
    setGoodsReceiptLines([]);
  }, []);

  const closeSearchModal = useCallback(() => {
    setShowSearchModal(false);
    setGoodsReceiptList([]);
    setActivePurchaseOrder(null);
  }, []);

  const openEditWithReceipt = useCallback(
    async (receiptId: number, poDetail: PurchaseOrderDto) => {
      setLoading(true);
      setShowEditModal(true);
      try {
        const receipt = await goodsReceiptsApi.getById(receiptId);
        const poLineMap = new Map(poDetail.lines.map((line) => [line.id, line]));
        const editableLines = receipt.lines.map((line) =>
          mapReceiptLineToEditable(line, poLineMap.get(line.purchaseOrderLineId)),
        );
        setGoodsReceipt(receipt);
        setGoodsReceiptLines(editableLines);
      } catch (error) {
        console.error('Failed to load goods receipt', error);
        showToast('Failed to load goods receipt', 'error');
        closeEditModal();
      } finally {
        setLoading(false);
      }
    },
    [closeEditModal, showToast],
  );

  const openNewDraftFromPo = useCallback(
    (poDetail: PurchaseOrderDto, initialQty: number, suffix?: string) => {
      const editableLines = poDetail.lines.map((line) => mapPoLineToGrLine(line, initialQty));
      setGoodsReceipt(buildNewDraftReceipt(poDetail.id, suffix ?? ''));
      setGoodsReceiptLines(editableLines);
      setShowEditModal(true);
      setLoading(false);
    },
    [],
  );

  const openApprovedPoFlow = useCallback(
    async (poDetail: PurchaseOrderDto) => {
      setActivePurchaseOrder(poDetail);
      setLoading(true);
      setShowEditModal(true);
      try {
        const receipts = await goodsReceiptsApi.getByPurchaseOrder(poDetail.id);
        const draftReceipt = receipts.find((r) => isGoodsReceiptDraft(r.status));

        if (draftReceipt) {
          await openEditWithReceipt(draftReceipt.id, poDetail);
          return;
        }

        openNewDraftFromPo(poDetail, 0);
      } catch (error) {
        console.error('Failed to open goods receipt for approved PO', error);
        showToast('Failed to open goods receipt', 'error');
        closeEditModal();
      } finally {
        setLoading(false);
      }
    },
    [closeEditModal, openEditWithReceipt, openNewDraftFromPo, showToast],
  );

  const openSearchFlow = useCallback(async (poDetail: PurchaseOrderDto) => {
    setActivePurchaseOrder(poDetail);
    setShowSearchModal(true);
    try {
      const receipts = await goodsReceiptsApi.getByPurchaseOrder(poDetail.id);
      setGoodsReceiptList(receipts);
    } catch (error) {
      console.error('Failed to load goods receipt list', error);
      showToast('Failed to load goods receipts', 'error');
      setGoodsReceiptList([]);
    }
  }, [showToast]);

  const openGoodsReceiptFromPo = useCallback(
    async (poDetail: PurchaseOrderDto) => {
      if (!canOpenGoodsReceipt(poDetail.status)) {
        showToast('Purchase order must be approved before creating goods receipt', 'error');
        return;
      }

      try {
        const receipts = await goodsReceiptsApi.getByPurchaseOrder(poDetail.id);
        const hasPostedReceipt = receipts.some((r) => isGoodsReceiptPosted(r.status));

        if (hasPostedReceipt) {
          await openSearchFlow(poDetail);
          return;
        }

        if (isPoApproved(poDetail.status)) {
          await openApprovedPoFlow(poDetail);
          return;
        }

        // Partially received but no posted receipt yet — fall back to approved flow
        await openApprovedPoFlow(poDetail);
      } catch (error) {
        console.error('Failed to open goods receipt flow', error);
        showToast('Failed to open goods receipt', 'error');
      }
    },
    [openApprovedPoFlow, openSearchFlow, showToast],
  );

  const handleSelectGoodsReceipt = useCallback(
    async (receiptId: number) => {
      if (!activePurchaseOrder) return;
      setShowSearchModal(false);
      await openEditWithReceipt(receiptId, activePurchaseOrder);
    },
    [activePurchaseOrder, openEditWithReceipt],
  );

  const handleCreateNewGoodsReceipt = useCallback(async () => {
    if (!activePurchaseOrder) return;

    setShowSearchModal(false);
    setLoading(true);
    setShowEditModal(true);

    try {
      const existingReceipts = await goodsReceiptsApi.getByPurchaseOrder(activePurchaseOrder.id);
      const suffix = String(existingReceipts.length + 1).padStart(2, '0');
      openNewDraftFromPo(activePurchaseOrder, 0, suffix);
    } catch (error) {
      console.error('Failed to create new goods receipt', error);
      showToast('Failed to create goods receipt', 'error');
      closeEditModal();
    } finally {
      setLoading(false);
    }
  }, [activePurchaseOrder, closeEditModal, openNewDraftFromPo, showToast]);

  const handleLineChange = useCallback(
    (index: number, quantityReceived: number) => {
      setGoodsReceiptLines((prev) => {
        const updated = [...prev];
        updated[index] = {
          ...updated[index],
          quantityReceived,
          lineTotal: quantityReceived * updated[index].unitCost,
        };
        return updated;
      });
    },
    [],
  );

  const handleSave = useCallback(async () => {
    if (!goodsReceipt) return;

    const linesToSave = buildLinesForSave(goodsReceiptLines);
    if (goodsReceipt.id === 0 && linesToSave.length === 0) {
      showToast('Enter quantity for at least one line before saving', 'error');
      return;
    }

    try {
      if (goodsReceipt.id === 0) {
        await goodsReceiptsApi.create({
          purchaseOrderId: goodsReceipt.purchaseOrderId,
          notes: goodsReceipt.notes,
          lines: linesToSave,
          postImmediately: false,
        });
        showToast('Goods receipt created successfully!', 'success');
      } else {
        await goodsReceiptsApi.update(goodsReceipt.id, {
          notes: goodsReceipt.notes,
          lines: linesToSave,
        });
        showToast('Goods receipt updated successfully!', 'success');
      }

      closeEditModal();
      await onPurchaseOrderChanged?.();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      showToast(getToastMessage(apiError), 'error');
    }
  }, [closeEditModal, goodsReceipt, goodsReceiptLines, onPurchaseOrderChanged, showToast]);

  const handlePost = useCallback(async () => {
    if (!goodsReceipt || goodsReceipt.id === 0) return;

    try {
      await goodsReceiptsApi.post(goodsReceipt.id);
      showToast('Goods receipt posted successfully!', 'success');
      closeEditModal();
      await onPurchaseOrderChanged?.();
    } catch (error: unknown) {
      const apiError = parseApiError(error);
      showToast(getToastMessage(apiError), 'error');
    }
  }, [closeEditModal, goodsReceipt, onPurchaseOrderChanged, showToast]);

  const isGoodsReceiptButtonDisabled = useCallback((status: string) => {
    return isPoDraftOrCancelled(status);
  }, []);

  return {
    showEditModal,
    showSearchModal,
    goodsReceipt,
    goodsReceiptLines,
    loading,
    goodsReceiptList,
    activePurchaseOrder,
    openGoodsReceiptFromPo,
    isGoodsReceiptButtonDisabled,
    handleSelectGoodsReceipt,
    handleCreateNewGoodsReceipt,
    handleLineChange,
    handleSave,
    handlePost,
    closeEditModal,
    closeSearchModal,
    setGoodsReceipt,
  };
}
