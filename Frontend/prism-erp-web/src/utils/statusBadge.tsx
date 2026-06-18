/** Normalize status string to a CSS class suffix (lowercase, alphanumeric only). */
export function getStatusClass(status: string | number): string {
  return String(status).replace(/[^a-zA-Z0-9]/g, '').toLowerCase();
}

export function StatusBadge({ status, label }: { status: string | number; label?: string }) {
  const text = label ?? String(status);
  return (
    <span className={`status ${getStatusClass(status)}`}>{text}</span>
  );
}
