import { UserPlus } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Outlet, useNavigate } from 'react-router';

import { useUsers } from '@/api/hooks/useUsers';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { PageHeader, Toolbar } from '@/components/layout';
import { Button } from '@/components/ui/button';
import { useErrorContext } from '@/contexts';
import type { SiteUser } from '@/data/siteUser';
import { WithPermission } from '@/features/auth/components';
import { usePermissions } from '@/hooks';
import { logger } from '@/lib/logging';

import { UserRoleDialog, UsersTable } from './components';

function UsersPage() {
  const { hasAnyPermission } = usePermissions();
  const navigate = useNavigate();
  const [userRoleDialogOpen, setUserRoleDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<SiteUser | null>(null);
  const { error, setError } = useErrorContext();

  const { data: usersData, isLoading } = useUsers();

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

  if (!canViewUsers) {
    return <div>You do not have permission to view users.</div>;
  }

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <PageHeader
        title="Users"
        subtitle="Manage user accounts and permissions"
      />
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <Toolbar>
          <div className="flex items-center gap-4">
            {/* Search functionality can be added here later */}
          </div>
          <WithPermission permissions={['user:manage']} mode="all">
            <Button
              onClick={() => navigate('invite')}
              aria-label="Invite a new user"
              className="gap-2 min-w-[132px]"
            >
              <UserPlus className="h-4 w-4" />
              Invite User
            </Button>
          </WithPermission>
        </Toolbar>

        <div className="flex-1 min-h-0 flex flex-col">
          <UsersTable users={users} onChangeRole={handleChangeRole} />
        </div>
      </div>

      <LoadingMessage isLoading={isLoading} />

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
