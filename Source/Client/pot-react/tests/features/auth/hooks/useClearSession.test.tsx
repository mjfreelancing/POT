import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useLogout } from '@/api/hooks/useAuth';
import { tokenProvider } from '@/concerns';
import type { User } from '@/data/user';
import {
  AccessTokenProvider,
  useAccessToken,
} from '@/features/auth/contexts/AccessTokenContext';
import useClearSession from '@/features/auth/hooks/useClearSession';
import { useUserStore } from '@/stores';

import { createUser } from '../../../shared/factories/userFactory';
import { createQueryClient } from '../../../shared/react-query/queryHookWrapper';

vi.mock('@/api/services/authService', () => ({
  refreshAccessToken: vi.fn(async () => ({ success: false })),
}));

vi.mock('@/api/hooks/useAuth', () => ({
  useLogout: vi.fn(),
}));

const signedInUser: User = createUser({ username: 'maria' });

function Probe() {
  const { setAccessToken } = useAccessToken();
  const clearSession = useClearSession();

  return (
    <>
      <button type="button" onClick={() => setAccessToken('token-123')}>
        Start session
      </button>
      <button type="button" onClick={clearSession}>
        Clear session
      </button>
    </>
  );
}

function renderProbe() {
  const queryClient = createQueryClient();

  render(
    <QueryClientProvider client={queryClient}>
      <AccessTokenProvider>
        <Probe />
      </AccessTokenProvider>
    </QueryClientProvider>,
  );

  return queryClient;
}

describe('useClearSession', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    useUserStore.setState({ userInfo: signedInUser });
    tokenProvider.setAccessToken(undefined);
  });

  test('clears the access token, the cached user and the query cache', async () => {
    const queryClient = renderProbe();

    queryClient.setQueryData(['users'], [signedInUser]);

    await userEvent.click(
      screen.getByRole('button', { name: 'Start session' }),
    );
    await waitFor(() =>
      expect(tokenProvider.getAccessToken()).toBe('token-123'),
    );

    await userEvent.click(screen.getByRole('button', { name: 'Clear session' }));

    await waitFor(() => expect(tokenProvider.getAccessToken()).toBeUndefined());
    expect(useUserStore.getState().userInfo).toBeNull();
    expect(queryClient.getQueryCache().getAll()).toHaveLength(0);
  });

  test('does not ask the server to log out', async () => {
    renderProbe();

    await userEvent.click(screen.getByRole('button', { name: 'Clear session' }));

    // The session is already gone on the server, so the teardown must never compose the logout request.
    expect(useLogout).not.toHaveBeenCalled();
  });
});
