import { vi } from 'vitest';

import type { Permission } from '@/concerns';

export type PermissionsApi = {
  permissions: Permission[];
  hasPermission: ReturnType<typeof vi.fn>;
  hasAllPermissions: ReturnType<typeof vi.fn>;
  hasAnyPermission: ReturnType<typeof vi.fn>;
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
