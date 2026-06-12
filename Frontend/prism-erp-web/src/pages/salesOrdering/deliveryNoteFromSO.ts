import { useState, useCallback } from 'react';
import { deliveryNotesApi } from '../../services/salesOrderingApi';
import type {
  SalesOrderDto,
  SalesOrderLineDto,
  DeliveryNoteDto,
  DeliveryNoteSummaryDto,
} from '../../services/types/salesOrdering.types';
import { parseApiError, getToastMessage } from '../../utils/errorHandler';

export interface DeliveryNoteLineEditable {
  id: number;
  salesOrderLineId: number;
  productId: number;
  productName: string;
  quantityOrdered: number;
  quantityRemaining: number;
  quantityDelivered: number;
}

export function normalizeStatus(status: string): string {
  return status.toLowerCase().replace(/[^a-z]/g, '');
}

export function isSoDraftOrCancelled(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'draft' || s === 'cancelled';
}

export function isSoConfirmed(status: string): boolean {
  return normalizeStatus(status) === 'confirmed';
}

export function isSoPartialDelivery(status: string): boolean {
  return normalizeStatus(status) === 'partialdelivery';
}

export function canOpenDeliveryNote(status: string): boolean {
  const s = normalizeStatus(status);
  return s === 'confirmed' || s === 'partialdelivery' || s === 'delivered';
}

export function isDeliveryNotePosted(status: string): boolean {
  return normalizeStatus(status) === 'posted';
}

export function isDeliveryNoteDraft(status: string): boolean {
  return normalizeStatus(status) === 'draft';
}

function mapSoLineToDnLine(
  line: SalesOrderLineDto,
  quantityDelivered: number,
  lineId = 0,
): DeliveryNoteLineEditable {
  return {
    id: lineId,
    salesOrderLineId: line.id,
    productId: line.productId,
    productName: line.productName,
    quantityOrdered: line.quantityOrdered,
    quantityRemaining: line.quantityRemaining,
    quantityDelivered,
  };
}

function mapDeliveryLineToEditable(
  deliveryLine: DeliveryNoteDto['lines'][number],
  soLine?: SalesOrderLineDto,
): DeliveryNoteLineEditable {
  return {
    id: deliveryLine.id,
    salesOrderLineId: deliveryLine.salesOrderLineId,
    productId: deliveryLine.productId,
    productName: deliveryLine.productName,
    quantityOrdered: soLine?.quantityOrdered ?? 0,
    quantityRemaining: soLine?.quantityRemaining ?? 0,
    quantityDelivered: deliveryLine.quantityDelivered,
  };
}

function buildNewDraftDelivery(salesOrder: SalesOrderDto): DeliveryNoteDto {
  return {
    id: 0,
    deliveryNumber: `DN-${new Date().getFullYear()}-${String(salesOrder.id).padStart(4, '0')}`,
    salesOrderId: salesOrder.id,
    customerId: salesOrder.customerId,
    customerName: salesOrder.customerName,
    orderNumber: salesOrder.orderNumber,
    deliveryDate: new Date().toISOString(),
    status: 'Draft',
    notes: '',
    lines: [],
  };
}

function buildLinesForSave(lines: DeliveryNoteLineEditable[]) {
  return lines
    .filter((line) => line.quantityDelivered > 0)
    .map((line) => ({
      salesOrderLineId: line.salesOrderLineId,
      quantityDelivered: line.quantityDelivered,
    }));
}

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

  const closeEditModal = useCallback(() => {
    setShowEditModal(false);
    setDeliveryNote(null);
    setDeliveryNoteLines([]);
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
    }
  }, [closeEditModal, deliveryNote, deliveryNoteLines, onSalesOrderChanged, showToast]);

  const handlePost = useCallback(async () => {
    if (!deliveryNote || deliveryNote.id === 0) return;

    try {
      await deliveryNotesApi.post(deliveryNote.id);
      showToast('Delivery note posted successfully!', 'success');
      closeEditModal();
      await onSalesOrderChanged?.();
    } catch (error: unknown) {
      showToast(getToastMessage(parseApiError(error)), 'error');
    }
  }, [closeEditModal, deliveryNote, onSalesOrderChanged, showToast]);

  const isDeliveryNoteButtonDisabled = useCallback((status: string) => {
    return isSoDraftOrCancelled(status);
  }, []);

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
    closeEditModal,
    closeSearchModal,
    setDeliveryNote,
  };
}
