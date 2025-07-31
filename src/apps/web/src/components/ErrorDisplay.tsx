import { AlertCircle } from 'lucide-react';
import { Button } from './ui/button';
import { ApiError } from '@/services/api-client';

interface ErrorDisplayProps {
  error: ApiError | Error | unknown;
  onRetry?: () => void;
  message?: string;
}

export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({ error, onRetry, message }) => {
  const getErrorMessage = () => {
    if (message) return message;
    
    if (error instanceof ApiError) {
      if (error.problem.detail) return error.problem.detail;
      if (error.problem.title) return error.problem.title;
    }
    
    if (error instanceof Error) {
      return error.message;
    }
    
    return 'An unexpected error occurred';
  };

  return (
    <div className="flex items-center justify-center py-8">
      <div className="text-center">
        <AlertCircle className="h-8 w-8 text-destructive mx-auto mb-2" />
        <p className="text-sm text-muted-foreground">{getErrorMessage()}</p>
        {onRetry && (
          <Button variant="outline" size="sm" className="mt-2" onClick={onRetry}>
            Try Again
          </Button>
        )}
      </div>
    </div>
  );
};