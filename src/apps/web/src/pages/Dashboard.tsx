import React, { useState, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { useToast } from '@/contexts/ToastContext';
import { 
  useUsers, 
  useMyVehicles, 
  useSyncVehicles, 
  useUnlinkTeslaAccount, 
  useRefreshTeslaTokens,
  useInitiateTeslaAuth 
} from '@/hooks/api/useAuth';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Spinner } from '@/components/ui/spinner';
import { Check, X, RefreshCw, LogOut, Link, Unlink, Car, AlertCircle } from 'lucide-react';
import { formatDate } from '@/utils/date';

export const Dashboard: React.FC = () => {
  const { user, logout, refreshUser } = useAuth();
  const { showError } = useToast();
  const [linkingTesla, setLinkingTesla] = useState(false);

  // API Hooks
  const { data: users, isLoading: isLoadingUsers, refetch: refetchUsers, error: usersError } = useUsers(!!user);
  const { data: vehicles, isLoading: isLoadingVehicles, refetch: refetchVehicles, error: vehiclesError } = useMyVehicles(!!user?.teslaAccount);
  const syncVehiclesMutation = useSyncVehicles();
  const unlinkTeslaMutation = useUnlinkTeslaAccount();
  const refreshTokensMutation = useRefreshTeslaTokens();
  const initiateTeslaAuthMutation = useInitiateTeslaAuth();

  // Listen for success message from popup
  useEffect(() => {
    const handleMessage = async (event: MessageEvent) => {
      if (event.origin === window.location.origin && event.data?.type === 'tesla-linked-success') {
        await refreshUser();
        await refetchUsers();
        await refetchVehicles();
      }
    };

    window.addEventListener('message', handleMessage);
    return () => window.removeEventListener('message', handleMessage);
  }, [refreshUser, refetchUsers, refetchVehicles]);

  const handleLinkTesla = async () => {
    try {
      setLinkingTesla(true);
      const result = await initiateTeslaAuthMutation.mutateAsync();
      
      // Open Tesla auth in a new window
      const popup = window.open(result.authUrl, 'tesla-auth', 'width=500,height=600');
      
      // Monitor the popup window
      const checkInterval = setInterval(async () => {
        if (popup && popup.closed) {
          clearInterval(checkInterval);
          // Refresh user data after popup closes
          await refreshUser();
          await refetchUsers();
          await refetchVehicles();
        }
      }, 500);
    } catch {
      showError('Failed to initiate Tesla authentication');
    } finally {
      setLinkingTesla(false);
    }
  };

  const handleUnlinkTesla = async () => {
    await unlinkTeslaMutation.mutateAsync();
    await refreshUser();
  };

  const renderErrorMessage = (error: unknown) => (
    <div className="flex items-center justify-center py-8">
      <div className="text-center">
        <AlertCircle className="h-8 w-8 text-destructive mx-auto mb-2" />
        <p className="text-sm text-muted-foreground">
          {(error as { problem?: { detail?: string } })?.problem?.detail || 'Failed to load data'}
        </p>
        <Button 
          variant="outline" 
          size="sm" 
          className="mt-2"
          onClick={() => window.location.reload()}
        >
          Try Again
        </Button>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen bg-background">
      <div className="container mx-auto py-10">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-4xl font-bold">Dashboard</h1>
          <Button onClick={logout} variant="outline" size="sm">
            <LogOut className="mr-2 h-4 w-4" />
            Logout
          </Button>
        </div>

        <div className="grid gap-6">
          {/* User Profile Card */}
          <Card>
            <CardHeader>
              <CardTitle>Your Profile</CardTitle>
              <CardDescription>Manage your account settings and Tesla connection</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="grid grid-cols-3 gap-4">
                  <div className="text-sm font-medium text-muted-foreground">Email</div>
                  <div className="col-span-2 text-sm">{user?.email}</div>
                </div>
                <div className="grid grid-cols-3 gap-4">
                  <div className="text-sm font-medium text-muted-foreground">Name</div>
                  <div className="col-span-2 text-sm">{user?.name}</div>
                </div>
                <div className="grid grid-cols-3 gap-4">
                  <div className="text-sm font-medium text-muted-foreground">Tesla Account</div>
                  <div className="col-span-2">
                    {user?.teslaAccount ? (
                      <div className="flex items-center gap-2">
                        <span className="text-sm text-green-600 flex items-center">
                          <Check className="mr-1 h-4 w-4" />
                          Connected
                        </span>
                        <Button
                          onClick={() => refreshTokensMutation.mutate()}
                          variant="ghost"
                          size="sm"
                          disabled={refreshTokensMutation.isPending}
                        >
                          <RefreshCw className={`mr-2 h-4 w-4 ${refreshTokensMutation.isPending ? 'animate-spin' : ''}`} />
                          Refresh
                        </Button>
                        <Button
                          onClick={handleUnlinkTesla}
                          variant="ghost"
                          size="sm"
                          className="text-destructive"
                          disabled={unlinkTeslaMutation.isPending}
                        >
                          <Unlink className="mr-2 h-4 w-4" />
                          Unlink
                        </Button>
                      </div>
                    ) : (
                      <Button
                        onClick={handleLinkTesla}
                        disabled={linkingTesla || initiateTeslaAuthMutation.isPending}
                        size="sm"
                      >
                        <Link className="mr-2 h-4 w-4" />
                        {linkingTesla ? 'Linking...' : 'Link Tesla Account'}
                      </Button>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-3 gap-4">
                  <div className="text-sm font-medium text-muted-foreground">Member Since</div>
                  <div className="col-span-2 text-sm">
                    {formatDate(user?.createdAt)}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* My Vehicles Card */}
          {user?.teslaAccount && (
            <Card>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <div>
                    <CardTitle>My Vehicles</CardTitle>
                    <CardDescription>Manage your Tesla vehicles</CardDescription>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      onClick={() => syncVehiclesMutation.mutate()}
                      variant="outline"
                      size="sm"
                      disabled={syncVehiclesMutation.isPending}
                    >
                      <RefreshCw className={`mr-2 h-4 w-4 ${syncVehiclesMutation.isPending ? 'animate-spin' : ''}`} />
                      {syncVehiclesMutation.isPending ? 'Syncing...' : 'Sync from Tesla'}
                    </Button>
                    <Button
                      onClick={() => refetchVehicles()}
                      variant="outline"
                      size="sm"
                    >
                      <RefreshCw className="mr-2 h-4 w-4" />
                      Refresh
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                {vehiclesError ? (
                  renderErrorMessage(vehiclesError)
                ) : isLoadingVehicles ? (
                  <div className="flex justify-center py-8">
                    <Spinner />
                  </div>
                ) : vehicles && vehicles.length > 0 ? (
                  <div className="grid gap-4">
                    {vehicles.map((vehicle) => (
                      <div key={vehicle.id} className="flex items-center justify-between p-4 border rounded-lg">
                        <div className="flex items-center gap-3">
                          <Car className="h-5 w-5 text-muted-foreground" />
                          <div>
                            <p className="font-medium">
                              {vehicle.displayName || vehicle.vehicleIdentifier}
                            </p>
                            <p className="text-sm text-muted-foreground">
                              VIN: {vehicle.vehicleIdentifier}
                            </p>
                            <p className="text-sm text-muted-foreground">
                              Linked: {formatDate(vehicle.linkedAt)}
                              {vehicle.lastSyncedAt && (
                                <> • Last synced: {formatDate(vehicle.lastSyncedAt)}</>
                              )}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          {vehicle.isActive ? (
                            <span className="text-sm text-green-600 flex items-center">
                              <Check className="mr-1 h-4 w-4" />
                              Active
                            </span>
                          ) : (
                            <span className="text-sm text-muted-foreground flex items-center">
                              <X className="mr-1 h-4 w-4" />
                              Inactive
                            </span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8">
                    <Car className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                    <p className="text-muted-foreground">No vehicles found</p>
                    <p className="text-sm text-muted-foreground mt-2">
                      Your vehicles will appear here once they're detected through the Tesla API
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* All Users Card */}
          <Card>
            <CardHeader>
              <div className="flex justify-between items-center">
                <div>
                  <CardTitle>All Users</CardTitle>
                  <CardDescription>View all registered users in the system</CardDescription>
                </div>
                <Button
                  onClick={() => refetchUsers()}
                  variant="outline"
                  size="sm"
                >
                  <RefreshCw className="mr-2 h-4 w-4" />
                  Refresh
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              {usersError ? (
                renderErrorMessage(usersError)
              ) : isLoadingUsers ? (
                <div className="flex justify-center py-8">
                  <Spinner />
                </div>
              ) : users && users.length > 0 ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Email</TableHead>
                      <TableHead>Name</TableHead>
                      <TableHead>Tesla Linked</TableHead>
                      <TableHead>Joined</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {users.map((u) => (
                      <TableRow key={u.id}>
                        <TableCell className="font-medium">{u.email}</TableCell>
                        <TableCell>{u.name}</TableCell>
                        <TableCell>
                          {u.teslaAccount ? (
                            <span className="text-green-600 flex items-center">
                              <Check className="mr-1 h-4 w-4" />
                            </span>
                          ) : (
                            <span className="text-muted-foreground flex items-center">
                              <X className="mr-1 h-4 w-4" />
                            </span>
                          )}
                        </TableCell>
                        <TableCell>{formatDate(u.createdAt)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <p className="text-center text-muted-foreground py-8">No users found</p>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};