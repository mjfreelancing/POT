import { cloneElement, type HTMLProps, type ReactElement } from 'react';

import { usePermissions } from '@/hooks';
import type { Permission } from '@/lib/permissions';

/**
 * A component that renders its child with a disabled state when the user
 * lacks the required permissions. This provides a consistent way to show
 * functionality exists but is not available to the current user.
 *
 * Use this component for interactive elements (buttons, inputs, etc.)
 * where you want to show the functionality exists but is not available
 * to the current user.
 *
 * @example
 * Multiple permissions (all required):
 * ```tsx
 * <WithPermission permissions={["account:manage", "account:view"]} mode="all">
 *   <Button>Create Account</Button>
 * </WithPermission>
 * ```
 *
 * @example
 * Multiple permissions (any required):
 * ```tsx
 * <WithPermission permissions={["account:manage", "account:view"]} mode="any">
 *   <Button>Account Actions</Button>
 * </WithPermission>
 * ```
 */
type WithPermissionProps = {
  permissions: Permission[];
  /**
   * Permission check mode:
   * - 'all': User must have ALL permissions (AND logic)
   * - 'any': User must have ANY permission (OR logic)
   */
  mode: 'all' | 'any';
  children: ReactElement<
    HTMLProps<HTMLButtonElement> | HTMLProps<HTMLInputElement>
  >;
};

function WithPermission({ permissions, mode, children }: WithPermissionProps) {
  const { hasAllPermissions, hasAnyPermission } = usePermissions();

  const hasAccess =
    mode === 'any'
      ? hasAnyPermission(permissions)
      : hasAllPermissions(permissions);

  if (hasAccess) {
    return children;
  }

  return (
    <div className="cursor-not-allowed">
      {cloneElement(children, {
        ...children.props,
        disabled: true,
        'aria-disabled': true,
        className:
          `${children.props.className || ''} pointer-events-none`.trim(),
      })}
    </div>
  );
}

export { WithPermission };
