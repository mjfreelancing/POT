import { useCallback } from 'react';

import type { Permission } from '@/lib/permissions';
import { EMPTY_PERMISSION_ARRAY } from '@/lib/types';
import { useUserStore } from '@/stores';

function usePermissions() {
  const permissions =
    useUserStore(state => state.userInfo?.permissions) ||
    EMPTY_PERMISSION_ARRAY;

  const hasPermission = useCallback(
    (permission: Permission) => {
      return permissions.includes(permission);
    },
    [permissions],
  );

  const hasAnyPermission = useCallback(
    (permissions: Permission[]) => {
      return permissions.some(hasPermission);
    },
    [hasPermission],
  );

  const hasAllPermissions = useCallback(
    (permissions: Permission[]) => {
      return permissions.every(hasPermission);
    },
    [hasPermission],
  );

  return {
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    permissions,
  };
}

export default usePermissions;
