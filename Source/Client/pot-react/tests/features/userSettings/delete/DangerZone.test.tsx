import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useAuthContext } from '@/features/auth/contexts';
import useClearSession from '@/features/auth/hooks/useClearSession';
import DangerZone from '@/features/userSettings/delete/DangerZone';
import useDeleteCurrentSite from '@/features/userSettings/delete/hooks/useDeleteCurrentSite';
import useDeleteOwnAccount from '@/features/userSettings/delete/hooks/useDeleteOwnAccount';
import { usePermissions } from '@/hooks';

import {
  createPermissionsApi,
  type PermissionsApi,
} from '../../../shared/auth/permissionsTestHelpers';

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => vi.fn(),
  };
});

vi.mock('@/hooks', () => ({
  usePermissions: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/features/auth/hooks/useClearSession', () => ({
  default: vi.fn(),
}));

vi.mock('@/features/auth/sessionNotice', () => ({
  setSessionNotice: vi.fn(),
}));

vi.mock('@/features/userSettings/delete/hooks/useDeleteOwnAccount', () => ({
  default: vi.fn(),
}));

vi.mock('@/features/userSettings/delete/hooks/useDeleteCurrentSite', () => ({
  default: vi.fn(),
}));

function createPermissions(options: { canManageSite: boolean }): PermissionsApi {
  const permissionsApi = createPermissionsApi();

  permissionsApi.hasPermission.mockImplementation(
    permission => options.canManageSite && permission === 'site:manage',
  );

  return permissionsApi;
}

async function expandDangerZone() {
  await userEvent.click(screen.getByRole('button', { name: /Danger Zone/ }));
}

describe('DangerZone', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuthContext).mockReturnValue({
      userInfo: {
        username: 'maria',
        site: { name: 'Site A' },
      },
    } as ReturnType<typeof useAuthContext>);

    vi.mocked(useClearSession).mockReturnValue(vi.fn());
    vi.mocked(useDeleteOwnAccount).mockReturnValue({
      deleteOwnAccount: vi.fn(),
    });
    vi.mocked(useDeleteCurrentSite).mockReturnValue({
      deleteCurrentSite: vi.fn(),
    });
  });

  test('keeps its actions collapsed until the zone is opened', () => {
    vi.mocked(usePermissions).mockReturnValue(
      createPermissions({ canManageSite: true }),
    );

    render(<DangerZone />);

    expect(screen.getByRole('button', { name: /Danger Zone/ })).toHaveAttribute(
      'aria-expanded',
      'false',
    );
    expect(
      screen.queryByRole('button', { name: 'Delete my account' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete this site' }),
    ).not.toBeInTheDocument();
  });

  test('lists the account action first for a user who cannot manage the site', async () => {
    vi.mocked(usePermissions).mockReturnValue(
      createPermissions({ canManageSite: false }),
    );

    render(<DangerZone />);
    await expandDangerZone();

    expect(
      screen.getByRole('button', { name: 'Delete my account' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete this site' }),
    ).not.toBeInTheDocument();
  });

  test('adds the site action after the account action for a site administrator', async () => {
    vi.mocked(usePermissions).mockReturnValue(
      createPermissions({ canManageSite: true }),
    );

    render(<DangerZone />);
    await expandDangerZone();

    // The account action stays in the same place for every role, with the wider-blast-radius action
    // escalating below it.
    const actions = screen.getAllByRole('button', {
      name: /Delete my account|Delete this site/,
    });

    expect(actions.map(action => action.textContent)).toEqual([
      'Delete my account',
      'Delete this site',
    ]);
  });
});
