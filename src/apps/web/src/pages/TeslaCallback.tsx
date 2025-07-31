import React, { useEffect, useState, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { authService } from '@/services/auth.service';
import { useAuth } from '@/contexts/AuthContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { CheckCircle2, XCircle, Loader2 } from 'lucide-react';
import { ApiError } from '@/services/api-client';

export const TeslaCallback: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { refreshUser } = useAuth();
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const hasProcessed = useRef(false);

  useEffect(() => {
    const handleCallback = async () => {
      // Prevent multiple executions (React StrictMode)
      if (hasProcessed.current) {
        return;
      }
      hasProcessed.current = true;
      const code = searchParams.get('code');
      const state = searchParams.get('state');
      const error = searchParams.get('error');

      if (error) {
        setStatus('error');
        setErrorMessage(`Tesla authorization failed: ${error}`);
        return;
      }

      if (!code || !state) {
        setStatus('error');
        setErrorMessage('Missing authorization code or state');
        return;
      }

      try {
        const result = await authService.handleTeslaCallback({ code, state });
        if (!result.success) {
          throw result.error;
        }
        
        await refreshUser();
        setStatus('success');
        
        // Close window if it's a popup, otherwise redirect
        if (window.opener) {
          // Try to trigger a refresh in the parent window before closing
          try {
            window.opener.postMessage({ type: 'tesla-linked-success' }, window.location.origin);
          } catch (e) {
            console.error('Failed to post message to opener:', e);
          }
          setTimeout(() => window.close(), 1000);
        } else {
          setTimeout(() => navigate('/dashboard'), 2000);
        }
      } catch (error) {
        setStatus('error');
        if (error instanceof ApiError) {
          setErrorMessage(error.problem.detail || error.problem.title || 'Failed to link Tesla account');
        } else {
          setErrorMessage('Failed to link Tesla account');
        }
      }
    };

    handleCallback();
  }, [searchParams, navigate, refreshUser]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="w-full max-w-md px-4">
        <Card>
          <CardHeader>
            <CardTitle className="text-center">
              {status === 'processing' && 'Linking Tesla Account'}
              {status === 'success' && 'Success!'}
              {status === 'error' && 'Linking Failed'}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-center">
              {status === 'processing' && (
                <>
                  <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto" />
                  <p className="mt-4 text-muted-foreground">
                    Please wait while we complete the process...
                  </p>
                </>
              )}

              {status === 'success' && (
                <>
                  <CheckCircle2 className="h-12 w-12 text-green-600 mx-auto" />
                  <p className="mt-4 text-muted-foreground">
                    {window.opener ? 'You can close this window' : 'Redirecting to dashboard...'}
                  </p>
                </>
              )}

              {status === 'error' && (
                <>
                  <XCircle className="h-12 w-12 text-destructive mx-auto" />
                  <p className="mt-4 text-destructive">{errorMessage}</p>
                  <Button
                    onClick={() => navigate('/dashboard')}
                    className="mt-6"
                  >
                    Back to Dashboard
                  </Button>
                </>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};