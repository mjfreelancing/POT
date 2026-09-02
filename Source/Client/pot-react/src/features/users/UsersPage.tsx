import { useQueryClient } from '@tanstack/react-query';
import { CheckCircle, Mail, UserPlus } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Outlet, useNavigate } from 'react-router';
import { toast } from 'sonner';

import {
  useResendInvitation,
  useUpdateUserStatus,
  useUsers,
} from '@/api/hooks';
import { ErrorSheet, LoadingOverlay } from '@/components/feedback';
import { SuccessToast } from '@/components/feedback/toast';
import { PageHeader, Toolbar } from '@/components/layout';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { logger, useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { SiteUser } from '@/data/siteUser';
import { WithPermission } from '@/features/auth/components';
import { useAuthContext } from '@/features/auth/contexts';
import { useIsMobile, usePermissions } from '@/hooks';
import { useIsShortViewport } from '@/hooks/use-short-viewport';

import { UserCardGrid, UserRoleDialog, UsersTable } from './components';

function UsersPage() {
  const { hasAnyPermission } = usePermissions();
  const { userInfo } = useAuthContext();
  const navigate = useNavigate();
  const isMobile = useIsMobile();
  const isShortViewport = useIsShortViewport();

  // On short viewports (e.g. phone landscape) the fixed header + toolbar can
  // crowd out the table region entirely, so scroll the whole content area below
  // the header instead of only the table/card region.
  const contentAreaClass = isShortViewport
    ? 'flex-1 min-h-0 flex flex-col gap-4 p-6 overflow-y-auto'
    : 'flex-1 min-h-0 flex flex-col gap-4 p-6';
  const listRegionClass = isShortViewport
    ? 'relative'
    : 'flex-1 min-h-0 flex flex-col relative';
  const [userRoleDialogOpen, setUserRoleDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<SiteUser | null>(null);
  const { error, setError } = useErrorContext();

  const {
    data: usersData,
    isLoading: usersLoading,
    isFetching: usersFetching,
  } = useUsers();

  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const resendInvitationMutation = useResendInvitation();
  const updateStatusMutation = useUpdateUserStatus();

  const isLoading = useMemo(
    () => usersLoading || usersFetching,
    [usersLoading, usersFetching],
  );

  useEffect(() => {
    logger.info('UsersPage', 'Mounted');

    return () => {
      logger.info('UsersPage', 'Unmounted');
    };
  }, []);

  // Check permissions
  const canViewUsers = hasAnyPermission(['user:view', 'user:manage']);

  // Extract users array from response
  const users = usersData?.success ? usersData.value : [];

  const handleChangeRole = (user: SiteUser) => {
    setSelectedUser(user);
    setUserRoleDialogOpen(true);
  };

  const handleToggleStatus = async (user: SiteUser) => {
    const newStatus = user.status === 'Enabled' ? 'Disabled' : 'Enabled';

    const result = await updateStatusMutation.mutateAsync({
      id: user.rowId,
      data: {
        etag: user.etag,
        status: newStatus,
      },
    });

    if (result.success) {
      invalidateCache(['users']);

      toast(
        <SuccessToast
          icon={CheckCircle}
          title="Status Updated"
          description={`${user.username} has been ${newStatus.toLowerCase()}.`}
        />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error('UsersPage', 'Failed to update user status', result.error);
    }
  };

  const handleResendInvitation = async (user: SiteUser) => {
    const result = await resendInvitationMutation.mutateAsync({
      id: user.rowId,
    });

    if (result.success) {
      toast(
        <SuccessToast
          icon={Mail}
          title="Invitation Resent"
          description={`Invitation has been sent to ${user.username}.`}
        />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error('UsersPage', 'Failed to resend invitation', result.error);
    }
  };

  // Handle errors
  useEffect(() => {
    if (usersData) {
      setError(
        usersData.success
          ? null
          : {
              title: usersData.error.code,
              description: usersData.error.description,
            },
      );
    }
  }, [usersData, setError]);

  // Only show permission denial when not loading and no error
  if (!canViewUsers && !isLoading && !error) {
    return <div>You do not have permission to view users.</div>;
  }

  return (
    <div className="flex flex-col h-screen overflow-hidden bg-gradient-to-br from-background to-muted/20">
      <PageHeader
        title="Users"
        subtitle="Manage user accounts and permissions"
      />
      <div className={contentAreaClass}>
        <Toolbar>
          <Badge variant="secondary" className="px-3 py-1.5 text-sm">
            <span className="font-semibold">{users.length}</span>{' '}
            {users.length === 1 ? 'User' : 'Users'}
          </Badge>
          <div className="shrink-0">
            <WithPermission permissions={['user:manage']} mode="all">
              <Button
                onClick={() => navigate('invite')}
                aria-label="Invite a new user"
                className="gap-2 min-w-[132px] w-full sm:w-auto"
              >
                <UserPlus className="h-4 w-4" />
                Invite User
              </Button>
            </WithPermission>
          </div>
        </Toolbar>

        <div className={listRegionClass}>
          {isLoading && <LoadingOverlay />}
          {isMobile ? (
            <UserCardGrid
              users={users}
              onChangeRole={handleChangeRole}
              onToggleStatus={handleToggleStatus}
              onResendInvitation={handleResendInvitation}
              currentUsername={userInfo?.username}
            />
          ) : (
            <UsersTable users={users} onChangeRole={handleChangeRole} />
          )}
        </div>
      </div>

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <UserRoleDialog
        user={selectedUser}
        isOpen={userRoleDialogOpen}
        onClose={() => setUserRoleDialogOpen(false)}
      />

      {/* The Outlet is used to render nested routes, such as when inviting users */}
      <Outlet />
    </div>
  );
}

export default UsersPage;
