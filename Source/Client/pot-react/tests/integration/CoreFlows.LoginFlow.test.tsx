import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { AuthenticationError } from '@/api/errors/apiErrors';
import { useLogin } from '@/api/hooks/useAuth';
import { useCacheInvalidation } from '@/concerns';
import { useAuthContext } from '@/features/auth/contexts';
import { FailResult, SuccessResult } from '@/lib';
import { AppRoutes } from '@/routes/AppRoutes';

type AuthState = {
  isAuthenticated: boolean;
  isInitialized: boolean;
  accessToken: string | undefined;
};

const authState: AuthState = {
  isAuthenticated: false,
  isInitialized: true,
  accessToken: undefined,
};

const mutateAsyncMock = vi.fn();
const logoutMock = vi.fn();
const invalidateCacheMock = vi.fn();

vi.mock('@/api/hooks/useAuth', () => ({
  useLogin: vi.fn(),
}));

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
  },
  logoutManager: {
    logout: vi.fn(),
  },
  useCacheInvalidation: vi.fn(),
}));

vi.mock('@/features/dashboard/DashboardPage', () => ({
  default: () => <h1>Dashboard page</h1>,
}));

function renderLoginFlow(initialPath: string = '/login') {
  const queryClient = new QueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <AppRoutes />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('Core Flow Integration - Login', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    authState.isAuthenticated = false;
    authState.isInitialized = true;
    authState.accessToken = undefined;

    vi.mocked(useAuthContext).mockImplementation(() => ({
      isAuthenticated: authState.isAuthenticated,
      isInitialized: authState.isInitialized,
      accessToken: authState.accessToken,
      userInfo: null,
      login: (token: string) => {
        authState.accessToken = token;
        authState.isAuthenticated = true;
      },
      logout: logoutMock,
    }));

    vi.mocked(useLogin).mockReturnValue({
      mutateAsync: mutateAsyncMock,
      isPending: false,
      data: undefined,
      mutate: vi.fn(),
      reset: vi.fn(),
    } as unknown as ReturnType<typeof useLogin>);

    vi.mocked(useCacheInvalidation).mockReturnValue(invalidateCacheMock);
  });

  test('successful auth redirects to dashboard', async () => {
    // Step 1: Mock a successful login transport response.
    mutateAsyncMock.mockResolvedValueOnce(
      new SuccessResult({
        status: 'Success' as const,
        accessToken: 'integration-access-token',
      }),
    );

    // Step 2: Start from the login route using real routing and real login page UI.
    renderLoginFlow('/login');

    // Step 3: Fill in credentials and submit the login form.
    await userEvent.type(screen.getByLabelText('Username'), 'integration.user');
    await userEvent.type(screen.getByLabelText('Password'), 'Secret123!');
    await userEvent.click(screen.getByRole('button', { name: 'Login' }));

    // Step 4: Verify the user is redirected through route protection to dashboard.
    expect(
      await screen.findByRole('heading', { name: 'Dashboard page' }),
    ).toBeInTheDocument();

    // Step 5: Verify post-login cache invalidation contract is executed.
    expect(invalidateCacheMock).toHaveBeenCalledWith(['me']);
  });

  test('authentication failure keeps user on login and shows error', async () => {
    // Step 1: Mock an authentication failure transport response.
    mutateAsyncMock.mockResolvedValueOnce(
      new FailResult(new AuthenticationError('Invalid username or password')),
    );

    // Step 2: Start from login with the same real routing/page setup.
    renderLoginFlow('/login');

    // Step 3: Submit invalid credentials.
    await userEvent.type(screen.getByLabelText('Username'), 'bad.user');
    await userEvent.type(screen.getByLabelText('Password'), 'wrong-pass');
    await userEvent.click(screen.getByRole('button', { name: 'Login' }));

    // Step 4: Verify error is visible and user remains on login experience.
    expect(
      await screen.findByText('Invalid username or password'),
    ).toBeInTheDocument();
    expect(screen.getByText('Login to your account')).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Dashboard page' }),
    ).not.toBeInTheDocument();
  });
});
