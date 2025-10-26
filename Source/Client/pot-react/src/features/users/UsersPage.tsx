import { useEffect } from 'react';

import { PageHeader } from '@/components/layout';
import { usePermissions } from '@/hooks';
import { logger } from '@/lib/logging';

function UsersPage() {
  const { hasAnyPermission } = usePermissions();

  useEffect(() => {
    logger.info('UsersPage', 'Mounted');

    return () => {
      logger.info('UsersPage', 'Unmounted');
    };
  }, []);

  // Only show page if user has either user:manage OR user:view permission
  if (!hasAnyPermission(['user:manage', 'user:view'])) {
    return null;
  }

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <PageHeader
        title="User Management"
        subtitle="Manage user accounts and permissions"
      />
      {/* Main content area: flex-1 min-h-0 for proper flexbox scrolling */}
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <div className="bg-card border rounded-lg p-6">
          <p className="text-muted-foreground">
            User management functionality will be implemented here.
          </p>
        </div>
      </div>
    </div>
  );
}

export default UsersPage;
