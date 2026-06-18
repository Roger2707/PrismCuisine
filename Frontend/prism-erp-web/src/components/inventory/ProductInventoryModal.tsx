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

type TabId = 'balance' | 'reservations' | 'movements' | 'costlayers';

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
    } catch {
      setError('No inventory balance for this product at selected warehouse.');
    } finally {
      setLoading(false);
    }
  }, [product, warehouseId]);

  useEffect(() => {
    if (product && warehouseId) {
      loadBalance();
      setActiveTab('balance');
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
    if (balance && activeTab !== 'balance') loadTabData(activeTab);
  }, [activeTab, balance, loadTabData]);

  if (!product) return null;

  const tabs: { id: TabId; label: string }[] = [
    { id: 'balance', label: 'Balance' },
    { id: 'reservations', label: 'Reservations' },
    { id: 'movements', label: 'Movements' },
    { id: 'costlayers', label: 'Cost Layers' },
  ];

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
                disabled={!balance && tab.id !== 'balance'}
              >
                {tab.label}
              </button>
            ))}
          </div>

          <div className="inventory-tab-panel">
            {loading && <div className="loading">Loading...</div>}
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
          </div>
        </div>
        <div className="modal-footer">
          <button className="cancel-button" onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}
