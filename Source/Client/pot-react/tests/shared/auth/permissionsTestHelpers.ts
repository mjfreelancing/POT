import { vi, type Mock } from 'vitest';

import type { Permission } from '@/concerns';

// Structurally matches the shape returned by usePermissions (src/hooks/usePermissions.ts)
// while keeping the vi.fn() mock instance so tests can assert call arguments.
export type PermissionsApi = {
  permissions: Permission[];
  hasPermission: Mock<(permission: Permission) => boolean>;
  hasAllPermissions: Mock<(permissions: Permission[]) => boolean>;
  hasAnyPermission: Mock<(permissions: Permission[]) => boolean>;
};

export function createPermissionsApi(
  options: {
    hasAll?: boolean;
    hasAny?: boolean;
  } = {},
): PermissionsApi {
  const { hasAll = false, hasAny = false } = options;

  return {
    permissions: [],
    hasPermission: vi.fn(() => false),
    hasAllPermissions: vi.fn(() => hasAll),
    hasAnyPermission: vi.fn(() => hasAny),
  };
}
