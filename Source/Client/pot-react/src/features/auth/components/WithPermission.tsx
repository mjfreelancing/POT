import { cloneElement, type HTMLProps, type ReactElement } from 'react';

import { usePermissions } from '../usePermissions';

/**
 * A component that renders its child with a disabled state when the user
 * lacks the required permission(s). This provides a consistent way to show
 * functionality exists but is not available to the current user.
 *
 * Use this component for interactive elements (buttons, inputs, etc.)
 * where you want to show the functionality exists but is not available
 * to the current user.
 *
 * @example
 * Single permission:
 * ```tsx
 * <WithPermission permission="account:manage">
 *   <Button>Create Account</Button>
 * </WithPermission>
 * ```
 *
 * Multiple permissions (all required):
 * ```tsx
 * <WithPermission permission={["account:manage", "account:view"]}>
 *   <Button>Create Account</Button>
 * </WithPermission>
 * ```
 */
type WithPermissionProps = {
  permission: string | string[];
  children: ReactElement<
    HTMLProps<HTMLButtonElement> | HTMLProps<HTMLInputElement>
  >;
};

function WithPermission({ permission, children }: WithPermissionProps) {
  const { hasPermission, hasAllPermissions } = usePermissions();

  const hasAccess = Array.isArray(permission)
    ? hasAllPermissions(permission)
    : hasPermission(permission);

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
