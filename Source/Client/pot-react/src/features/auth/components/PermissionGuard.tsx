import { type ReactNode } from 'react';

import { usePermissions } from '@/hooks';

/**
 * A component that conditionally renders its children based on whether
 * the current user has a specific permission. If the permission check fails,
 * nothing is rendered (returns null).
 *
 * Use this component when you want to completely hide content from users
 * who don't have the required permission.
 */
type PermissionGuardProps = {
  /**
   * A single permission or array of permissions required to render the children.
   * If an array is provided, the user must have ALL permissions to see the content.
   */
  permission: string | string[];
  children: ReactNode;
};

function PermissionGuard({ permission, children }: PermissionGuardProps) {
  const { hasPermission, hasAllPermissions } = usePermissions();

  const hasAccess = Array.isArray(permission)
    ? hasAllPermissions(permission)
    : hasPermission(permission);

  return hasAccess ? <>{children}</> : null;
}

export { PermissionGuard };
