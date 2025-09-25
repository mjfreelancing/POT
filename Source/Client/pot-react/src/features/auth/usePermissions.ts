import { useCallback } from 'react';

import { useUserStore } from './stores/useUserStore';

function usePermissions() {
  // Using this to avoid warning 'The 'permissions' logical expression could make the dependencies of useCallback Hook (at line 12) change on every render.'
  const EMPTY_ARRAY: string[] = [];

  const permissions =
    useUserStore(state => state.userInfo?.permissions) || EMPTY_ARRAY;

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

export { usePermissions };
