import { useCallback } from 'react';

import { usePermissionStore } from './stores/usePermissionStore';

export function usePermissions() {
  const permissions = usePermissionStore(state => state.permissions);

  const hasPermission = useCallback(
    (permission: string) => {
      return permissions.includes(permission);
    },
    [permissions],
  );

  const hasAnyPermission = useCallback(
    (permissions: string[]) => {
      return permissions.some(hasPermission);
    },
    [hasPermission],
  );

  const hasAllPermissions = useCallback(
    (permissions: string[]) => {
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
