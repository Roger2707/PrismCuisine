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
  const [receipts, setReceipts] = useState<GoodsReceipt[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchReceipts = async () => {
      try {
        // Note: Backend doesn't have GetAll, only GetByPurchaseOrder and GetById
        // Using mock data for now
        setReceipts([
          { id: 1, receiptNumber: 'GRN-2024-001', purchaseOrder: 'PO-2024-001', supplier: 'Da Lat Fresh Vegetables Cooperative', totalAmount: 1500000, status: 'Posted', receiptDate: '2024-01-15' },
          { id: 2, receiptNumber: 'GRN-2024-002', purchaseOrder: 'PO-2024-002', supplier: 'An Binh Food Company', totalAmount: 2400000, status: 'Posted', receiptDate: '2024-01-16' },
          { id: 3, receiptNumber: 'GRN-2024-003', purchaseOrder: 'PO-2024-003', supplier: 'Nha Trang Fresh Seafood', totalAmount: 5400000, status: 'Draft', receiptDate: '2024-01-17' },
        ]);
      } catch (error) {
        console.log('API not available, using mock data');
        setReceipts([
          { id: 1, receiptNumber: 'GRN-2024-001', purchaseOrder: 'PO-2024-001', supplier: 'Da Lat Fresh Vegetables Cooperative', totalAmount: 1500000, status: 'Posted', receiptDate: '2024-01-15' },
          { id: 2, receiptNumber: 'GRN-2024-002', purchaseOrder: 'PO-2024-002', supplier: 'An Binh Food Company', totalAmount: 2400000, status: 'Posted', receiptDate: '2024-01-16' },
          { id: 3, receiptNumber: 'GRN-2024-003', purchaseOrder: 'PO-2024-003', supplier: 'Nha Trang Fresh Seafood', totalAmount: 5400000, status: 'Draft', receiptDate: '2024-01-17' },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchReceipts();
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
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
