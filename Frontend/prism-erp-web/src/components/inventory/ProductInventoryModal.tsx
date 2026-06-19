import { useState, useEffect, useCallback } from 'react';
import type { ProductDto } from '../../services/types/inventory.types';
import type {
  InventoryBalanceDto,
  InventoryMovementDto,
  InventoryCostLayerDto,
  InventoryReservationDto,
  WarehouseDto,
} from '../../services/types/inventory.types';
import { inventoryApi, warehousesApi } from '../../services/inventoryApi';
import { formatCurrency, formatDate } from '../../utils/formatters';
import { StatusBadge } from '../../utils/statusBadge';
import { LoadingButton } from '../LoadingButton';
import { parseApiError, getToastMessage } from '../../utils/errorHandler';

type TabId = 'balance' | 'reservations' | 'movements' | 'costlayers' | 'adjust';

interface ProductInventoryModalProps {
  product: ProductDto | null;
  onClose: () => void;
}

export function ProductInventoryModal({ product, onClose }: ProductInventoryModalProps) {
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [warehouseId, setWarehouseId] = useState<number>(0);
  const [activeTab, setActiveTab] = useState<TabId>('balance');
  const [loading, setLoading] = useState(false);
  const [balance, setBalance] = useState<InventoryBalanceDto | null>(null);
  const [reservations, setReservations] = useState<InventoryReservationDto[]>([]);
  const [movements, setMovements] = useState<InventoryMovementDto[]>([]);
  const [costLayers, setCostLayers] = useState<InventoryCostLayerDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [adjustNewQty, setAdjustNewQty] = useState('');
  const [adjustUnitCost, setAdjustUnitCost] = useState('');
  const [adjustReference, setAdjustReference] = useState('');
  const [adjustNotes, setAdjustNotes] = useState('');
  const [adjustError, setAdjustError] = useState<string | null>(null);
  const [adjustSuccess, setAdjustSuccess] = useState<string | null>(null);
  const [adjusting, setAdjusting] = useState(false);

  useEffect(() => {
    warehousesApi.getAll()
      .then((list) => {
        setWarehouses(list);
        if (list.length > 0) setWarehouseId(list[0].id);
      })
      .catch(() => setWarehouses([]));
  }, []);

  const loadBalance = useCallback(async () => {
    if (!product || !warehouseId) return;
    setLoading(true);
    setError(null);
    setBalance(null);
    setReservations([]);
    setMovements([]);
    setCostLayers([]);
    try {
      const bal = await inventoryApi.getBalance(product.id, warehouseId);
      setBalance(bal);
      setAdjustNewQty(String(bal.quantityOnHand));
    } catch {
      setError('No inventory balance for this product at selected warehouse.');
      setAdjustNewQty('0');
    } finally {
      setLoading(false);
    }
  }, [product, warehouseId]);

  useEffect(() => {
    if (product && warehouseId) {
      loadBalance();
      setActiveTab('balance');
      setAdjustError(null);
      setAdjustSuccess(null);
    }
  }, [product, warehouseId, loadBalance]);

  const loadTabData = useCallback(async (tab: TabId) => {
    if (!balance) return;
    setLoading(true);
    try {
      if (tab === 'reservations') {
        setReservations(await inventoryApi.getReservations(balance.id));
      } else if (tab === 'movements') {
        setMovements(await inventoryApi.getMovements(balance.id));
      } else if (tab === 'costlayers') {
        setCostLayers(await inventoryApi.getCostLayers(balance.id));
      }
    } catch {
      setError('Failed to load data.');
    } finally {
      setLoading(false);
    }
  }, [balance]);

  useEffect(() => {
    if (balance && activeTab !== 'balance' && activeTab !== 'adjust') {
      loadTabData(activeTab);
    }
  }, [activeTab, balance, loadTabData]);

  const handleAdjust = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!product || !warehouseId) return;

    setAdjustError(null);
    setAdjustSuccess(null);

    const newQuantity = Number(adjustNewQty);
    const unitCostForIncrease = Number(adjustUnitCost) || 0;

    if (Number.isNaN(newQuantity) || newQuantity < 0) {
      setAdjustError('New quantity must be a non-negative number.');
      return;
    }

    setAdjusting(true);
    try {
      let workingBalance = balance;
      if (!workingBalance) {
        workingBalance = await inventoryApi.ensureBalance({
          productId: product.id,
          warehouseId,
          reorderLevel: 0,
        });
        setBalance(workingBalance);
      }

      await inventoryApi.adjust({
        productId: product.id,
        warehouseId,
        newQuantity,
        unitCostForIncrease,
        reference: adjustReference || undefined,
        notes: adjustNotes || undefined,
      });

      setAdjustSuccess('Inventory adjusted successfully.');
      await loadBalance();
      setActiveTab('balance');
    } catch (err: unknown) {
      setAdjustError(getToastMessage(parseApiError(err)));
    } finally {
      setAdjusting(false);
    }
  };

  if (!product) return null;

  const tabs: { id: TabId; label: string }[] = [
    { id: 'balance', label: 'Balance' },
    { id: 'reservations', label: 'Reservations' },
    { id: 'movements', label: 'Movements' },
    { id: 'costlayers', label: 'Cost Layers' },
    { id: 'adjust', label: 'Adjust' },
  ];

  const isIncrease =
    balance !== null
      ? Number(adjustNewQty) > balance.quantityOnHand
      : Number(adjustNewQty) > 0;

  return (
    <div className="modal-overlay">
      <div className="modal modal-large">
        <div className="modal-header">
          <h2>Inventory — {product.name}</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>
        <div className="modal-body">
          <div className="info-grid" style={{ marginBottom: '16px' }}>
            <div className="info-item"><label>SKU:</label><span>{product.sku}</span></div>
            <div className="info-item"><label>Unit:</label><span>{product.unit}</span></div>
            <div className="info-item">
              <label>Warehouse:</label>
              <select value={warehouseId} onChange={(e) => setWarehouseId(Number(e.target.value))}>
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>{w.name}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="inventory-tabs">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                type="button"
                className={`inventory-tab ${activeTab === tab.id ? 'active' : ''}`}
                onClick={() => setActiveTab(tab.id)}
                disabled={!balance && tab.id !== 'balance' && tab.id !== 'adjust'}
              >
                {tab.label}
              </button>
            ))}
          </div>

          <div className="inventory-tab-panel">
            {loading && activeTab !== 'adjust' && <div className="loading">Loading...</div>}
            {!loading && error && activeTab === 'balance' && (
              <p style={{ color: '#64748b', textAlign: 'center', padding: '24px' }}>{error}</p>
            )}
            {!loading && balance && activeTab === 'balance' && (
              <div className="info-grid">
                <div className="info-item"><label>On Hand:</label><span>{balance.quantityOnHand}</span></div>
                <div className="info-item"><label>Reserved:</label><span>{balance.reservedQuantity}</span></div>
                <div className="info-item"><label>Available:</label><span>{balance.availableQuantity}</span></div>
                <div className="info-item"><label>Reorder Level:</label><span>{balance.reorderLevel}</span></div>
                <div className="info-item">
                  <label>Low Stock:</label>
                  <StatusBadge status={balance.isBelowReorderLevel ? 'Warning' : 'OK'} label={balance.isBelowReorderLevel ? 'Yes' : 'No'} />
                </div>
              </div>
            )}
            {!loading && balance && activeTab === 'reservations' && (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>ID</th><th>Qty</th><th>Fulfilled</th><th>Remaining</th><th>Status</th><th>Reference</th>
                  </tr>
                </thead>
                <tbody>
                  {reservations.map((r) => (
                    <tr key={r.id}>
                      <td>{r.id}</td>
                      <td>{r.quantity}</td>
                      <td>{r.fulfilledQuantity}</td>
                      <td>{r.remainingQuantity}</td>
                      <td><StatusBadge status={r.status} /></td>
                      <td>{r.referenceType} #{r.referenceId}</td>
                    </tr>
                  ))}
                  {reservations.length === 0 && (
                    <tr><td colSpan={6} style={{ textAlign: 'center' }}>No reservations</td></tr>
                  )}
                </tbody>
              </table>
            )}
            {!loading && balance && activeTab === 'movements' && (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>ID</th><th>Type</th><th>Qty</th><th>Unit Cost</th><th>Reference</th><th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {movements.map((m) => (
                    <tr key={m.id}>
                      <td>{m.id}</td>
                      <td><StatusBadge status={m.movementType} /></td>
                      <td>{m.quantity}</td>
                      <td>{formatCurrency(m.unitCost)}</td>
                      <td>{m.referenceType} {m.reference || m.referenceId}</td>
                      <td>{formatDate(m.createdAt)}</td>
                    </tr>
                  ))}
                  {movements.length === 0 && (
                    <tr><td colSpan={6} style={{ textAlign: 'center' }}>No movements</td></tr>
                  )}
                </tbody>
              </table>
            )}
            {!loading && balance && activeTab === 'costlayers' && (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>ID</th><th>Received</th><th>Remaining</th><th>Unit Cost</th><th>Received At</th>
                  </tr>
                </thead>
                <tbody>
                  {costLayers.map((l) => (
                    <tr key={l.id}>
                      <td>{l.id}</td>
                      <td>{l.quantityReceived}</td>
                      <td>{l.quantityRemaining}</td>
                      <td>{formatCurrency(l.unitCost)}</td>
                      <td>{formatDate(l.receivedAt)}</td>
                    </tr>
                  ))}
                  {costLayers.length === 0 && (
                    <tr><td colSpan={5} style={{ textAlign: 'center' }}>No cost layers</td></tr>
                  )}
                </tbody>
              </table>
            )}
            {activeTab === 'adjust' && (
              <form onSubmit={handleAdjust} className="form-grid" style={{ maxWidth: '480px' }}>
                <p style={{ color: '#64748b', marginBottom: '8px' }}>
                  Set the physical count after stocktake. Current on-hand:{' '}
                  <strong>{balance?.quantityOnHand ?? 0}</strong>
                </p>
                <div className="form-group">
                  <label htmlFor="adjust-new-qty">New quantity</label>
                  <input
                    id="adjust-new-qty"
                    type="number"
                    min="0"
                    step="any"
                    value={adjustNewQty}
                    onChange={(e) => setAdjustNewQty(e.target.value)}
                    required
                  />
                </div>
                {isIncrease && (
                  <div className="form-group">
                    <label htmlFor="adjust-unit-cost">Unit cost (for increase)</label>
                    <input
                      id="adjust-unit-cost"
                      type="number"
                      min="0"
                      step="any"
                      value={adjustUnitCost}
                      onChange={(e) => setAdjustUnitCost(e.target.value)}
                      required={isIncrease}
                    />
                  </div>
                )}
                <div className="form-group">
                  <label htmlFor="adjust-reference">Reference</label>
                  <input
                    id="adjust-reference"
                    type="text"
                    value={adjustReference}
                    onChange={(e) => setAdjustReference(e.target.value)}
                    placeholder="Stocktake #..."
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="adjust-notes">Notes</label>
                  <textarea
                    id="adjust-notes"
                    value={adjustNotes}
                    onChange={(e) => setAdjustNotes(e.target.value)}
                    rows={2}
                  />
                </div>
                {adjustError && <div className="error-message">{adjustError}</div>}
                {adjustSuccess && <div className="success-message">{adjustSuccess}</div>}
                <LoadingButton type="submit" loading={adjusting} loadingText="Adjusting...">
                  Apply adjustment
                </LoadingButton>
              </form>
            )}
          </div>
        </div>
        <div className="modal-footer">
          <button className="cancel-button" onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}
