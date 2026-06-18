import type { ButtonHTMLAttributes, ReactNode } from 'react';

interface LoadingButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  loading?: boolean;
  loadingText?: string;
  children: ReactNode;
  variant?: 'primary' | 'secondary' | 'approve' | 'danger' | 'action';
}

const variantClass: Record<NonNullable<LoadingButtonProps['variant']>, string> = {
  primary: 'save-button',
  secondary: 'cancel-button',
  approve: 'approve-button',
  danger: 'action-btn delete',
  action: 'action-btn goods-receipt',
};

export function LoadingButton({
  loading = false,
  loadingText = 'Loading...',
  children,
  variant = 'primary',
  disabled,
  className = '',
  ...rest
}: LoadingButtonProps) {
  return (
    <button
      {...rest}
      className={`${variantClass[variant]} ${loading ? 'btn-loading' : ''} ${className}`.trim()}
      disabled={disabled || loading}
    >
      {loading ? loadingText : children}
    </button>
  );
}
