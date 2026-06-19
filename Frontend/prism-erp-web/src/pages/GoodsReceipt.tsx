import { useState, useEffect } from 'react';
import './Inventory.css';

interface GoodsReceipt {
  id: number;
  receiptNumber: string;
  purchaseOrder: string;
  supplier: string;
  totalAmount: number;
  status: string;
  receiptDate: string;
}

export default function GoodsReceipt() {
  const [receipts] = useState<GoodsReceipt[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(false);
  }, []);

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>📥 Goods Receipt Module</h1>
        <p>Manage goods receipts from suppliers</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>Goods Receipt List</h2>
            <button className="add-button">+ Create Receipt</button>
          </div>

          <p style={{ color: '#64748b', padding: '16px' }}>
            Goods receipts are managed from Purchase Order. Backend list API is not available yet.
          </p>

          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Receipt Number</th>
                <th>Purchase Order</th>
                <th>Supplier</th>
                <th>Total Amount</th>
                <th>Status</th>
                <th>Receipt Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {receipts.map((receipt) => (
                <tr key={receipt.id}>
                  <td>{receipt.id}</td>
                  <td>{receipt.receiptNumber}</td>
                  <td>{receipt.purchaseOrder}</td>
                  <td>{receipt.supplier}</td>
                  <td>₫{receipt.totalAmount.toLocaleString()}</td>
                  <td>
                    <span className={`status ${receipt.status.toLowerCase()}`}>
                      {receipt.status}
                    </span>
                  </td>
                  <td>{receipt.receiptDate}</td>
                  <td>
                    <button className="action-btn edit">Details</button>
                  </td>
                </tr>
              ))}
              {receipts.length === 0 && (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center' }}>No goods receipts</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
