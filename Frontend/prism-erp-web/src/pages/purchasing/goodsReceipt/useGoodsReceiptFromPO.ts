import { useState, useCallback } from 'react';
import { goodsReceiptsApi } from '../../../services/purchasingApi';
import { purchaseInvoicesApi } from '../../../services/financeApi';
import type { InvoiceDto } from '../../../services/types/finance.types';
import type { PurchaseOrderDto, GoodsReceiptDto, GoodsReceiptSummaryDto } from '../../../services/types/purchasing.types';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import { confirmAction, ConfirmMessages } from '../../../utils/confirmAction';
import type { GoodsReceiptLineEditable } from './types';
import {
  isGoodsReceiptDraft,
  isGoodsReceiptPosted,
  isGoodsReceiptCancelled,
  canCancelGoodsReceipt,
  canOpenGoodsReceipt,
  isPoApproved,
  isPoDraftOrCancelled,
} from './statusHelpers';
import {
  mapPoLineToGrLine,
  mapReceiptLineToEditable,
  buildNewDraftReceipt,
  buildLinesForSave,
} from './mappers';

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
  const [linkedInvoice, setLinkedInvoice] = useState<InvoiceDto | null>(null);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [invoiceLoading, setInvoiceLoading] = useState(false);
  const [creatingInvoice, setCreatingInvoice] = useState(false);
  const [saving, setSaving] = useState(false);
  const [posting, setPosting] = useState(false);
  const [cancelling, setCancelling] = useState(false);

  const closeEditModal = useCallback(() => {
    setShowEditModal(false);
    setGoodsReceipt(null);
    setGoodsReceiptLines([]);
    setLinkedInvoice(null);
    setShowInvoiceModal(false);
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
        if (isGoodsReceiptPosted(receipt.status)) {
          const invoice = await purchaseInvoicesApi.getByGoodsReceipt(receiptId);
          setLinkedInvoice(invoice);
        } else {
          setLinkedInvoice(null);
        }
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

  const handleLineChange = useCallback((index: number, quantityReceived: number) => {
    setGoodsReceiptLines((prev) => {
      const updated = [...prev];
      updated[index] = {
        ...updated[index],
        quantityReceived,
        lineTotal: quantityReceived * updated[index].unitCost,
      };
      return updated;
    });
  }, []);

  const handleSave = useCallback(async () => {
    if (!goodsReceipt) return;
    const linesToSave = buildLinesForSave(goodsReceiptLines);
    if (goodsReceipt.id === 0 && linesToSave.length === 0) {
      showToast('Enter quantity for at least one line before saving', 'error');
      return;
    }
    if (!confirmAction(ConfirmMessages.saveGoodsReceipt)) return;
    setSaving(true);
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
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setSaving(false);
    }
  }, [closeEditModal, goodsReceipt, goodsReceiptLines, onPurchaseOrderChanged, showToast]);

  const handlePost = useCallback(async () => {
    if (!goodsReceipt || goodsReceipt.id === 0) return;
    if (!confirmAction(ConfirmMessages.postGoodsReceipt)) return;
    setPosting(true);
    try {
      await goodsReceiptsApi.post(goodsReceipt.id);
      showToast('Goods receipt posted successfully!', 'success');
      closeEditModal();
      await onPurchaseOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setPosting(false);
    }
  }, [closeEditModal, goodsReceipt, onPurchaseOrderChanged, showToast]);

  const handleCancelReceipt = useCallback(async () => {
    if (!goodsReceipt || goodsReceipt.id === 0) return;
    if (!canCancelGoodsReceipt(goodsReceipt.status)) return;
    if (!confirmAction(ConfirmMessages.cancelGoodsReceipt)) return;
    setCancelling(true);
    try {
      await goodsReceiptsApi.cancel(goodsReceipt.id);
      showToast('Goods receipt cancelled successfully!', 'success');
      closeEditModal();
      await onPurchaseOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setCancelling(false);
    }
  }, [closeEditModal, goodsReceipt, onPurchaseOrderChanged, showToast]);

  const isGoodsReceiptButtonDisabled = useCallback((status: string) => isPoDraftOrCancelled(status), []);

  const handleViewInvoice = useCallback(async () => {
    if (!goodsReceipt || goodsReceipt.id === 0) return;
    setShowInvoiceModal(true);
    setInvoiceLoading(true);
    try {
      const invoice = linkedInvoice ?? await purchaseInvoicesApi.getByGoodsReceipt(goodsReceipt.id);
      setLinkedInvoice(invoice);
      if (!invoice) showToast('Invoice not found for this goods receipt', 'error');
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setInvoiceLoading(false);
    }
  }, [goodsReceipt, linkedInvoice, showToast]);

  const handleCreateInvoice = useCallback(async () => {
    if (!goodsReceipt || goodsReceipt.id === 0) return;
    if (!confirmAction(ConfirmMessages.createPurchaseInvoice)) return;
    setCreatingInvoice(true);
    try {
      const invoice = await purchaseInvoicesApi.createFromGoodsReceipt({
        purchaseOrderId: goodsReceipt.purchaseOrderId,
        goodsReceiptId: goodsReceipt.id,
      });
      setLinkedInvoice(invoice);
      showToast('Invoice created successfully!', 'success');
      await onPurchaseOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setCreatingInvoice(false);
    }
  }, [goodsReceipt, onPurchaseOrderChanged, showToast]);

  const closeInvoiceModal = useCallback(() => setShowInvoiceModal(false), []);

  const isPostedReceipt = goodsReceipt ? isGoodsReceiptPosted(goodsReceipt.status) : false;
  const isCancelledReceipt = goodsReceipt ? isGoodsReceiptCancelled(goodsReceipt.status) : false;
  const canCancelReceipt = goodsReceipt ? canCancelGoodsReceipt(goodsReceipt.status) : false;
  const hasLinkedInvoice = linkedInvoice !== null;

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
    handleCancelReceipt,
    closeEditModal,
    closeSearchModal,
    setGoodsReceipt,
    linkedInvoice,
    showInvoiceModal,
    invoiceLoading,
    creatingInvoice,
    handleViewInvoice,
    handleCreateInvoice,
    closeInvoiceModal,
    isPostedReceipt,
    isCancelledReceipt,
    canCancelReceipt,
    hasLinkedInvoice,
    saving,
    posting,
    cancelling,
  };
}

export type { GoodsReceiptLineEditable } from './types';
export {
  normalizeStatus,
  isPoDraftOrCancelled,
  isPoApproved,
  isPoPartiallyReceived,
  isPoFullyReceived,
  canOpenGoodsReceipt,
  isGoodsReceiptPosted,
  isGoodsReceiptDraft,
  isGoodsReceiptCancelled,
  canCancelGoodsReceipt,
} from './statusHelpers';
