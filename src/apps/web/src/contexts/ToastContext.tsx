import React, { createContext, useContext, useState, useCallback } from 'react';
import { Toast, ToastContainer, ToastProps } from '@/components/ui/toast';

interface ToastContextValue {
  showToast: (options: Omit<ToastProps, 'id'>) => void;
  showError: (message: string, title?: string) => void;
  showSuccess: (message: string, title?: string) => void;
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

export const useToast = () => {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<ToastProps[]>([]);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback((options: Omit<ToastProps, 'id'>) => {
    const id = Date.now().toString();
    const toast: ToastProps = {
      ...options,
      id,
      onClose: () => removeToast(id),
    };

    setToasts((prev) => [...prev, toast]);

    // Auto-remove after duration
    const duration = options.duration ?? 5000;
    if (duration > 0) {
      setTimeout(() => removeToast(id), duration);
    }
  }, [removeToast]);

  const showError = useCallback((message: string, title = 'Error') => {
    showToast({
      title,
      description: message,
      variant: 'destructive',
    });
  }, [showToast]);

  const showSuccess = useCallback((message: string, title = 'Success') => {
    showToast({
      title,
      description: message,
      variant: 'success',
    });
  }, [showToast]);

  return (
    <ToastContext.Provider value={{ showToast, showError, showSuccess }}>
      {children}
      <ToastContainer>
        {toasts.map((toast) => (
          <Toast key={toast.id} {...toast} />
        ))}
      </ToastContainer>
    </ToastContext.Provider>
  );
};