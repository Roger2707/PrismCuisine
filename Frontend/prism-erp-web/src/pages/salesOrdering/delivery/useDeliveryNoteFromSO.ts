import { useState, useCallback } from 'react';
import { deliveryNotesApi } from '../../../services/salesOrderingApi';
import { invoicesApi } from '../../../services/financeApi';
import type { InvoiceDto } from '../../../services/types/finance.types';
import type { SalesOrderDto, DeliveryNoteDto, DeliveryNoteSummaryDto } from '../../../services/types/salesOrdering.types';
import { parseApiError, getToastMessage } from '../../../utils/errorHandler';
import { confirmAction, ConfirmMessages } from '../../../utils/confirmAction';
import type { DeliveryNoteLineEditable } from './types';
import {
  isDeliveryNoteDraft,
  isDeliveryNotePosted,
  isDeliveryNoteCancelled,
  canCancelDeliveryNote,
  canOpenDeliveryNote,
  isSoDraftOrCancelled,
} from './statusHelpers';
import { mapSoLineToDnLine, mapDeliveryLineToEditable, buildNewDraftDelivery, buildLinesForSave } from './mappers';

interface UseDeliveryNoteFromSOOptions {
  showToast: (message: string, type: 'success' | 'error') => void;
  onSalesOrderChanged?: () => Promise<void>;
}

export function useDeliveryNoteFromSO({ showToast, onSalesOrderChanged }: UseDeliveryNoteFromSOOptions) {
  const [showEditModal, setShowEditModal] = useState(false);
  const [showSearchModal, setShowSearchModal] = useState(false);
  const [deliveryNote, setDeliveryNote] = useState<DeliveryNoteDto | null>(null);
  const [deliveryNoteLines, setDeliveryNoteLines] = useState<DeliveryNoteLineEditable[]>([]);
  const [loading, setLoading] = useState(false);
  const [deliveryNoteList, setDeliveryNoteList] = useState<DeliveryNoteSummaryDto[]>([]);
  const [activeSalesOrder, setActiveSalesOrder] = useState<SalesOrderDto | null>(null);
  const [linkedInvoice, setLinkedInvoice] = useState<InvoiceDto | null>(null);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [invoiceLoading, setInvoiceLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [posting, setPosting] = useState(false);
  const [cancelling, setCancelling] = useState(false);

  const closeEditModal = useCallback(() => {
    setShowEditModal(false);
    setDeliveryNote(null);
    setDeliveryNoteLines([]);
    setLinkedInvoice(null);
    setShowInvoiceModal(false);
  }, []);

  const closeSearchModal = useCallback(() => {
    setShowSearchModal(false);
    setDeliveryNoteList([]);
    setActiveSalesOrder(null);
  }, []);

  const openEditWithDelivery = useCallback(
    async (deliveryId: number, soDetail: SalesOrderDto) => {
      setLoading(true);
      setShowEditModal(true);
      try {
        const note = await deliveryNotesApi.getById(deliveryId);
        const soLineMap = new Map(soDetail.lines.map((line) => [line.id, line]));
        const editableLines = note.lines.map((line) =>
          mapDeliveryLineToEditable(line, soLineMap.get(line.salesOrderLineId)),
        );
        setDeliveryNote(note);
        setDeliveryNoteLines(editableLines);
        if (isDeliveryNotePosted(note.status)) {
          const invoice = await invoicesApi.getByDeliveryNote(deliveryId);
          setLinkedInvoice(invoice);
        } else {
          setLinkedInvoice(null);
        }
      } catch (error) {
        console.error('Failed to load delivery note', error);
        showToast('Failed to load delivery note', 'error');
        closeEditModal();
      } finally {
        setLoading(false);
      }
    },
    [closeEditModal, showToast],
  );

  const openNewDraftFromSo = useCallback((soDetail: SalesOrderDto, initialQty: number) => {
    const editableLines = soDetail.lines.map((line) => mapSoLineToDnLine(line, initialQty));
    setDeliveryNote(buildNewDraftDelivery(soDetail));
    setDeliveryNoteLines(editableLines);
    setShowEditModal(true);
    setLoading(false);
  }, []);

  const openConfirmedSoFlow = useCallback(
    async (soDetail: SalesOrderDto) => {
      setActiveSalesOrder(soDetail);
      setLoading(true);
      setShowEditModal(true);
      try {
        const notes = await deliveryNotesApi.getBySalesOrder(soDetail.id);
        const draftNote = notes.find((n) => isDeliveryNoteDraft(n.status));
        if (draftNote) {
          await openEditWithDelivery(draftNote.id, soDetail);
          return;
        }
        openNewDraftFromSo(soDetail, 0);
      } catch (error) {
        console.error('Failed to open delivery note for confirmed SO', error);
        showToast('Failed to open delivery note', 'error');
        closeEditModal();
      } finally {
        setLoading(false);
      }
    },
    [closeEditModal, openEditWithDelivery, openNewDraftFromSo, showToast],
  );

  const openSearchFlow = useCallback(async (soDetail: SalesOrderDto) => {
    setActiveSalesOrder(soDetail);
    setShowSearchModal(true);
    try {
      const notes = await deliveryNotesApi.getBySalesOrder(soDetail.id);
      setDeliveryNoteList(notes);
    } catch (error) {
      console.error('Failed to load delivery note list', error);
      showToast('Failed to load delivery notes', 'error');
      setDeliveryNoteList([]);
    }
  }, [showToast]);

  const openDeliveryNoteFromSo = useCallback(
    async (soDetail: SalesOrderDto) => {
      if (!canOpenDeliveryNote(soDetail.status)) {
        showToast('Sales order must be confirmed before creating delivery note', 'error');
        return;
      }
      try {
        const notes = await deliveryNotesApi.getBySalesOrder(soDetail.id);
        const hasPostedNote = notes.some((n) => isDeliveryNotePosted(n.status));
        if (hasPostedNote) {
          await openSearchFlow(soDetail);
          return;
        }
        await openConfirmedSoFlow(soDetail);
      } catch (error) {
        console.error('Failed to open delivery note flow', error);
        showToast('Failed to open delivery note', 'error');
      }
    },
    [openConfirmedSoFlow, openSearchFlow, showToast],
  );

  const handleSelectDeliveryNote = useCallback(
    async (deliveryId: number) => {
      if (!activeSalesOrder) return;
      setShowSearchModal(false);
      await openEditWithDelivery(deliveryId, activeSalesOrder);
    },
    [activeSalesOrder, openEditWithDelivery],
  );

  const handleCreateNewDeliveryNote = useCallback(async () => {
    if (!activeSalesOrder) return;
    setShowSearchModal(false);
    setLoading(true);
    setShowEditModal(true);
    try {
      openNewDraftFromSo(activeSalesOrder, 0);
    } catch (error) {
      console.error('Failed to create new delivery note', error);
      showToast('Failed to create delivery note', 'error');
      closeEditModal();
    } finally {
      setLoading(false);
    }
  }, [activeSalesOrder, closeEditModal, openNewDraftFromSo, showToast]);

  const handleLineChange = useCallback((index: number, quantityDelivered: number) => {
    setDeliveryNoteLines((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index], quantityDelivered };
      return updated;
    });
  }, []);

  const handleSave = useCallback(async () => {
    if (!deliveryNote) return;
    const linesToSave = buildLinesForSave(deliveryNoteLines);
    if (deliveryNote.id === 0 && linesToSave.length === 0) {
      showToast('Enter quantity for at least one line before saving', 'error');
      return;
    }
    if (!confirmAction(ConfirmMessages.saveDeliveryNote)) return;
    setSaving(true);
    try {
      if (deliveryNote.id === 0) {
        await deliveryNotesApi.create({
          salesOrderId: deliveryNote.salesOrderId,
          notes: deliveryNote.notes,
          lines: linesToSave,
        });
        showToast('Delivery note created successfully!', 'success');
      } else {
        await deliveryNotesApi.update(deliveryNote.id, {
          notes: deliveryNote.notes,
          lines: linesToSave,
        });
        showToast('Delivery note updated successfully!', 'success');
      }
      closeEditModal();
      await onSalesOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setSaving(false);
    }
  }, [closeEditModal, deliveryNote, deliveryNoteLines, onSalesOrderChanged, showToast]);

  const handlePost = useCallback(async () => {
    if (!deliveryNote || deliveryNote.id === 0) return;
    if (!confirmAction(ConfirmMessages.postDeliveryNote)) return;
    setPosting(true);
    try {
      await deliveryNotesApi.post(deliveryNote.id);
      showToast('Delivery note posted successfully!', 'success');
      closeEditModal();
      await onSalesOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setPosting(false);
    }
  }, [closeEditModal, deliveryNote, onSalesOrderChanged, showToast]);

  const handleCancelDelivery = useCallback(async () => {
    if (!deliveryNote || deliveryNote.id === 0) return;
    if (!canCancelDeliveryNote(deliveryNote.status)) return;
    if (!confirmAction(ConfirmMessages.cancelDeliveryNote)) return;
    setCancelling(true);
    try {
      await deliveryNotesApi.cancel(deliveryNote.id);
      showToast('Delivery note cancelled successfully!', 'success');
      closeEditModal();
      await onSalesOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setCancelling(false);
    }
  }, [closeEditModal, deliveryNote, onSalesOrderChanged, showToast]);

  const isDeliveryNoteButtonDisabled = useCallback((status: string) => isSoDraftOrCancelled(status), []);

  const handleViewInvoice = useCallback(async () => {
    if (!deliveryNote || deliveryNote.id === 0) return;
    setShowInvoiceModal(true);
    setInvoiceLoading(true);
    try {
      const invoice = linkedInvoice ?? await invoicesApi.getByDeliveryNote(deliveryNote.id);
      setLinkedInvoice(invoice);
      if (!invoice) showToast('Invoice not found for this delivery note', 'error');
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    } finally {
      setInvoiceLoading(false);
    }
  }, [deliveryNote, linkedInvoice, showToast]);

  const closeInvoiceModal = useCallback(() => setShowInvoiceModal(false), []);

  const canViewInvoice = deliveryNote
    ? isDeliveryNotePosted(deliveryNote.status) && (linkedInvoice !== null || deliveryNote.id > 0)
    : false;

  const isCancelledDelivery = deliveryNote ? isDeliveryNoteCancelled(deliveryNote.status) : false;
  const canCancelDelivery = deliveryNote ? canCancelDeliveryNote(deliveryNote.status) : false;

  return {
    showEditModal,
    showSearchModal,
    deliveryNote,
    deliveryNoteLines,
    loading,
    deliveryNoteList,
    activeSalesOrder,
    openDeliveryNoteFromSo,
    isDeliveryNoteButtonDisabled,
    handleSelectDeliveryNote,
    handleCreateNewDeliveryNote,
    handleLineChange,
    handleSave,
    handlePost,
    handleCancelDelivery,
    closeEditModal,
    closeSearchModal,
    setDeliveryNote,
    linkedInvoice,
    showInvoiceModal,
    invoiceLoading,
    handleViewInvoice,
    closeInvoiceModal,
    canViewInvoice,
    isCancelledDelivery,
    canCancelDelivery,
    saving,
    posting,
    cancelling,
  };
}

// Re-export for backward compatibility
export type { DeliveryNoteLineEditable } from './types';
export {
  normalizeStatus,
  isSoDraftOrCancelled,
  isSoConfirmed,
  isSoPartialDelivery,
  canOpenDeliveryNote,
  isDeliveryNotePosted,
  isDeliveryNoteDraft,
  isDeliveryNoteCancelled,
  canCancelDeliveryNote,
} from './statusHelpers';
