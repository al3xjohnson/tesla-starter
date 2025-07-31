import React, { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Descope } from '@descope/react-sdk';
import { useAuth } from '@/contexts/AuthContext';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export const Login: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAuthenticated } = useAuth();

  const from = location.state?.from?.pathname || '/dashboard';

  useEffect(() => {
    if (isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, from]);

  const handleSuccess = () => {
    navigate(from, { replace: true });
  };

  const handleError = () => {
    console.error('Descope authentication error');
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="w-full max-w-md px-4">
        <Card>
          <CardHeader className="space-y-1">
            <CardTitle className="text-2xl text-center">Welcome to TeslaStarter</CardTitle>
            <CardDescription className="text-center">
              Sign in to connect and manage your Tesla vehicles
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Descope
              flowId="sign-up-or-in"
              onSuccess={handleSuccess}
              onError={handleError}
              theme="light"
            />
          </CardContent>
        </Card>
      </div>
    </div>
  );
};