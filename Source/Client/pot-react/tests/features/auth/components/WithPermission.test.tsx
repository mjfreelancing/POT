import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { WithPermission } from '@/features/auth/components/WithPermission';
import { usePermissions } from '@/hooks';
import { createPermissionsApi } from '../../../shared/auth/permissionsTestHelpers';

vi.mock('@/hooks', () => ({
  usePermissions: vi.fn(),
}));

describe('WithPermission', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('renders child unchanged when mode is all and permissions are granted', () => {
    const permissionsApi = createPermissionsApi({ hasAll: true });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <WithPermission
        permissions={['account:view', 'expense:manage']}
        mode="all"
      >
        <button type="button" className="base-action">
          Manage expense
        </button>
      </WithPermission>,
    );

    const actionButton = screen.getByRole('button', { name: 'Manage expense' });

    expect(actionButton).toBeEnabled();
    expect(actionButton).not.toHaveAttribute('aria-disabled', 'true');
    expect(actionButton).toHaveClass('base-action');
    expect(actionButton).not.toHaveClass('pointer-events-none');
    expect(actionButton.closest('div.cursor-not-allowed')).toBeNull();

    expect(permissionsApi.hasAllPermissions).toHaveBeenCalledWith([
      'account:view',
      'expense:manage',
    ]);
    expect(permissionsApi.hasAnyPermission).not.toHaveBeenCalled();
  });

  test('disables child and wraps with blocked cursor when mode is all and permissions are missing', () => {
    const permissionsApi = createPermissionsApi({ hasAll: false });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <WithPermission
        permissions={['account:view', 'expense:manage']}
        mode="all"
      >
        <button type="button" className="base-action">
          Manage expense
        </button>
      </WithPermission>,
    );

    const actionButton = screen.getByRole('button', { name: 'Manage expense' });

    expect(actionButton).toBeDisabled();
    expect(actionButton).toHaveAttribute('aria-disabled', 'true');
    expect(actionButton).toHaveClass('base-action', 'pointer-events-none');
    expect(actionButton.closest('div.cursor-not-allowed')).not.toBeNull();

    expect(permissionsApi.hasAllPermissions).toHaveBeenCalledWith([
      'account:view',
      'expense:manage',
    ]);
  });

  test('uses hasAnyPermission path when mode is any', () => {
    const permissionsApi = createPermissionsApi({ hasAny: false });
    vi.mocked(usePermissions).mockReturnValue(permissionsApi);

    render(
      <WithPermission permissions={['site:view', 'site:manage']} mode="any">
        <button type="button">Site action</button>
      </WithPermission>,
    );

    expect(permissionsApi.hasAnyPermission).toHaveBeenCalledWith([
      'site:view',
      'site:manage',
    ]);
    expect(permissionsApi.hasAllPermissions).not.toHaveBeenCalled();

    const actionButton = screen.getByRole('button', { name: 'Site action' });

    expect(actionButton).toBeDisabled();
  });
});
