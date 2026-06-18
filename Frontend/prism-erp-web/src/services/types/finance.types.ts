// Finance module types (mirrors BE Finance module)

export type InvoiceType = 'SalesInvoice' | 'PurchaseInvoice' | 'CreditNote' | 'DebitNote' | number;
export type FinanceInvoiceStatus = 'Unpaid' | 'PartialPaid' | 'Paid' | 'Cancelled' | number;
export type PaymentMethod =
  | 'Cash'
  | 'BankTransfer'
  | 'CreditCard'
  | 'DebitCard'
  | 'Check'
  | 'ElectronicTransfer'
  | number;
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded' | 'Cancelled' | number;

/** SO/PO invoicing progress (NotInvoiced | PartiallyInvoiced | FullyInvoiced) */
export type OrderInvoicingStatus = 'NotInvoiced' | 'PartiallyInvoiced' | 'FullyInvoiced' | string;

export interface InvoiceLineDto {
  id: number;
  invoiceId: number;
  productId: number;
  productName?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
  taxAmount: number;
  discountRate: number;
  discountAmount: number;
  lineTotal: number;
}

export interface InvoiceDto {
  id: number;
  invoiceNumber: string;
  invoiceType: InvoiceType;
  status: FinanceInvoiceStatus;
  invoiceDate: string;
  dueDate?: string;
  counterpartyName?: string;
  counterpartyAddress?: string;
  salesOrderId?: number;
  deliveryNoteId?: number;
  purchaseOrderId?: number;
  goodsReceiptId?: number;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  paidAmount: number;
  notes?: string;
  lines: InvoiceLineDto[];
}

export interface PaymentDto {
  id: number;
  invoiceId: number;
  paymentNumber: string;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  amount: number;
  paymentDate: string;
  referenceNumber?: string;
  bankName?: string;
  accountNumber?: string;
  notes?: string;
}

export interface CreatePaymentRequest {
  invoiceId: number;
  paymentNumber: string;
  paymentMethod: PaymentMethod;
  amount: number;
  paymentDate: string;
  referenceNumber?: string;
  bankName?: string;
  accountNumber?: string;
  notes?: string;
}

export interface UpdatePaymentRequest {
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  bankName?: string;
  accountNumber?: string;
  notes?: string;
}

export interface CreatePurchaseInvoiceRequest {
  purchaseOrderId: number;
  goodsReceiptId: number;
}

const INVOICE_TYPE_LABELS: Record<string, string> = {
  SalesInvoice: 'Sales Invoice',
  PurchaseInvoice: 'Purchase Invoice',
  CreditNote: 'Credit Note',
  DebitNote: 'Debit Note',
  '1': 'Sales Invoice',
  '2': 'Purchase Invoice',
  '3': 'Credit Note',
  '4': 'Debit Note',
};

const FINANCE_INVOICE_STATUS_LABELS: Record<string, string> = {
  Unpaid: 'Unpaid',
  PartialPaid: 'Partial Paid',
  Paid: 'Paid',
  Cancelled: 'Cancelled',
  '0': 'Unpaid',
  '1': 'Partial Paid',
  '2': 'Paid',
  '3': 'Cancelled',
};

const PAYMENT_METHOD_LABELS: Record<string, string> = {
  Cash: 'Cash',
  BankTransfer: 'Bank Transfer',
  CreditCard: 'Credit Card',
  DebitCard: 'Debit Card',
  Check: 'Check',
  ElectronicTransfer: 'Electronic Transfer',
  '1': 'Cash',
  '2': 'Bank Transfer',
  '3': 'Credit Card',
  '4': 'Debit Card',
  '5': 'Check',
  '6': 'Electronic Transfer',
};

const PAYMENT_STATUS_LABELS: Record<string, string> = {
  Pending: 'Pending',
  Completed: 'Completed',
  Failed: 'Failed',
  Refunded: 'Refunded',
  Cancelled: 'Cancelled',
  '1': 'Pending',
  '2': 'Completed',
  '3': 'Failed',
  '4': 'Refunded',
  '5': 'Cancelled',
};

const ORDER_INVOICING_STATUS_LABELS: Record<string, string> = {
  NotInvoiced: 'Not Invoiced',
  PartiallyInvoiced: 'Partially Invoiced',
  FullyInvoiced: 'Fully Invoiced',
};

export function formatInvoiceType(value: InvoiceType): string {
  return INVOICE_TYPE_LABELS[String(value)] ?? String(value);
}

export function formatFinanceInvoiceStatus(value: FinanceInvoiceStatus): string {
  return FINANCE_INVOICE_STATUS_LABELS[String(value)] ?? String(value);
}

export function formatPaymentMethod(value: PaymentMethod): string {
  return PAYMENT_METHOD_LABELS[String(value)] ?? String(value);
}

export function formatPaymentStatus(value: PaymentStatus): string {
  return PAYMENT_STATUS_LABELS[String(value)] ?? String(value);
}

export function formatOrderInvoicingStatus(value: OrderInvoicingStatus): string {
  return ORDER_INVOICING_STATUS_LABELS[String(value)] ?? String(value);
}

export function normalizeEnumValue(value: string | number): string {
  if (typeof value === 'number') {
    const maps: Record<number, string> = {
      0: 'Unpaid',
      1: 'PartialPaid',
      2: 'Paid',
      3: 'Cancelled',
    };
    return maps[value] ?? String(value);
  }
  return value;
}

export const PAYMENT_METHOD_OPTIONS = [
  { value: 1, label: 'Cash' },
  { value: 2, label: 'Bank Transfer' },
  { value: 3, label: 'Credit Card' },
  { value: 4, label: 'Debit Card' },
  { value: 5, label: 'Check' },
  { value: 6, label: 'Electronic Transfer' },
];
