import { useCallback } from 'react';

import { EMPTY_STRING_ARRAY } from '@/lib';
import { useUserStore } from '@/stores';

function usePermissions() {
  const permissions =
    useUserStore(state => state.userInfo?.permissions) || EMPTY_STRING_ARRAY;

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

export default usePermissions;
