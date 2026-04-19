import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { PermissionGuard } from '@/features/auth/components/PermissionGuard';
import { usePermissions } from '@/hooks';
import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';

vi.mock('@/hooks', () => ({
  usePermissions: vi.fn(),
}));

describe('PermissionGuard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('renders children when mode is all and all permissions are granted', () => {
    const permissionsApi = createPermissionsApi({ hasAll: true });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <PermissionGuard
        permissions={['account:view', 'expense:manage']}
        mode="all"
      >
        <div>Protected content</div>
      </PermissionGuard>,
    );

    expect(screen.getByText('Protected content')).toBeInTheDocument();
    expect(permissionsApi.hasAllPermissions).toHaveBeenCalledWith([
      'account:view',
      'expense:manage',
    ]);
    expect(permissionsApi.hasAnyPermission).not.toHaveBeenCalled();
  });

  test('does not render children when mode is all and one or more permissions are missing', () => {
    const permissionsApi = createPermissionsApi({ hasAll: false });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <PermissionGuard
        permissions={['account:view', 'expense:manage']}
        mode="all"
      >
        <div>Protected content</div>
      </PermissionGuard>,
    );

    expect(screen.queryByText('Protected content')).toBeNull();
    expect(permissionsApi.hasAllPermissions).toHaveBeenCalledWith([
      'account:view',
      'expense:manage',
    ]);
  });

  test('renders children when mode is any and at least one permission is granted', () => {
    const permissionsApi = createPermissionsApi({ hasAny: true });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <PermissionGuard
        permissions={['account:view', 'expense:manage']}
        mode="any"
      >
        <div>Protected content</div>
      </PermissionGuard>,
    );

    expect(screen.getByText('Protected content')).toBeInTheDocument();
    expect(permissionsApi.hasAnyPermission).toHaveBeenCalledWith([
      'account:view',
      'expense:manage',
    ]);
    expect(permissionsApi.hasAllPermissions).not.toHaveBeenCalled();
  });

  test('does not render children when mode is any and no permissions are granted', () => {
    const permissionsApi = createPermissionsApi({ hasAny: false });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <PermissionGuard
        permissions={['account:view', 'expense:manage']}
        mode="any"
      >
        <div>Protected content</div>
      </PermissionGuard>,
    );

    expect(screen.queryByText('Protected content')).toBeNull();
    expect(permissionsApi.hasAnyPermission).toHaveBeenCalledWith([
      'account:view',
      'expense:manage',
    ]);
  });
});
