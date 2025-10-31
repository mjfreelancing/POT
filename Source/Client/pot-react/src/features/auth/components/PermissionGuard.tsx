import { type ReactNode } from 'react';

import { usePermissions } from '@/hooks';
import type { Permission } from '@/lib/permissions';

/**
 * A component that conditionally renders its children based on whether
 * the current user has the required permissions. If the permission check fails,
 * nothing is rendered (returns null).
 *
 * Use this component when you want to completely hide content from users
 * who don't have the required permissions.
 */
type PermissionGuardProps = {
  /**
   * Array of permissions required to render the children.
   */
  permissions: Permission[];
  /**
   * Permission check mode:
   * - 'all': User must have ALL permissions (AND logic)
   * - 'any': User must have ANY permission (OR logic)
   */
  mode: 'all' | 'any';
  children: ReactNode;
};

function PermissionGuard({
  permissions,
  mode,
  children,
}: PermissionGuardProps) {
  const { hasAllPermissions, hasAnyPermission } = usePermissions();

  const hasAccess =
    mode === 'any'
      ? hasAnyPermission(permissions)
      : hasAllPermissions(permissions);

  return hasAccess ? <>{children}</> : null;
}

export { PermissionGuard };
