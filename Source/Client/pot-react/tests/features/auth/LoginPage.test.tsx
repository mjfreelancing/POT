import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { AuthenticationError, UnexpectedError } from '@/api/errors/apiErrors';
import { useLogin } from '@/api/hooks/useAuth';
import LoginPage from '@/features/auth/LoginPage';
import { useAuthContext } from '@/features/auth/contexts';
import { FailResult, SuccessResult } from '@/lib';

import { useCacheInvalidation } from '@/concerns';

const navigateMock = vi.fn();

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

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
  useCacheInvalidation: vi.fn(),
}));

function renderLoginPage() {
  const queryClient = new QueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('LoginPage', () => {
  const loginMock = vi.fn();
  const mutateAsyncMock = vi.fn();
  const invalidateCacheMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuthContext).mockReturnValue({
      accessToken: undefined,
      userInfo: null,
      isAuthenticated: false,
      isInitialized: true,
      login: loginMock,
      logout: vi.fn(),
    });

    vi.mocked(useLogin).mockReturnValue({
      mutateAsync: mutateAsyncMock,
      isPending: false,
      data: undefined,
      mutate: vi.fn(),
      reset: vi.fn(),
    } as unknown as ReturnType<typeof useLogin>);

    vi.mocked(useCacheInvalidation).mockReturnValue(invalidateCacheMock);
  });

  test('renders login page with real form content', () => {
    renderLoginPage();

    expect(screen.getByText('Login to your account')).toBeInTheDocument();
    expect(screen.getByLabelText('Username')).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Login' })).toBeDisabled();
  });

  test('submitting valid credentials triggers success flow and navigation', async () => {
    mutateAsyncMock.mockResolvedValue(
      new SuccessResult({
        status: 'Success' as const,
        accessToken: 'access-token-123',
      }),
    );

    renderLoginPage();

    await userEvent.type(screen.getByLabelText('Username'), '  malcolm  ');
    await userEvent.type(screen.getByLabelText('Password'), 'Secret123!');
    await userEvent.click(screen.getByRole('button', { name: 'Login' }));

    await waitFor(() => {
      expect(mutateAsyncMock).toHaveBeenCalledTimes(1);
    });

    expect(mutateAsyncMock).toHaveBeenCalledWith(
      expect.objectContaining({
        data: {
          username: 'malcolm',
          password: 'Secret123!',
        },
        timeoutMs: 120000,
      }),
    );

    await waitFor(() => {
      expect(loginMock).toHaveBeenCalledWith('access-token-123');
      expect(invalidateCacheMock).toHaveBeenCalledWith(['me']);
      expect(navigateMock).toHaveBeenCalledWith('/');
    });
  });

  test('shows approval message and does not navigate when account is pending approval', async () => {
    mutateAsyncMock.mockResolvedValue(
      new SuccessResult({
        status: 'Approval' as const,
        message: 'Your account is pending approval.',
      }),
    );

    renderLoginPage();

    await userEvent.type(screen.getByLabelText('Username'), 'pending.user');
    await userEvent.type(screen.getByLabelText('Password'), 'Secret123!');
    await userEvent.click(screen.getByRole('button', { name: 'Login' }));

    expect(
      await screen.findByText('Account Pending Approval'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Your account is pending approval.'),
    ).toBeInTheDocument();

    expect(loginMock).not.toHaveBeenCalled();
    expect(invalidateCacheMock).not.toHaveBeenCalled();
    expect(navigateMock).not.toHaveBeenCalled();
  });

  test('shows authentication error in the form for authentication failures', async () => {
    mutateAsyncMock.mockResolvedValue(
      new FailResult(new AuthenticationError('Invalid username or password')),
    );

    renderLoginPage();

    await userEvent.type(screen.getByLabelText('Username'), 'bad.user');
    await userEvent.type(screen.getByLabelText('Password'), 'wrong-pass');
    await userEvent.click(screen.getByRole('button', { name: 'Login' }));

    expect(
      await screen.findByText('Invalid username or password'),
    ).toBeInTheDocument();

    expect(navigateMock).not.toHaveBeenCalled();
  });

  test('shows and dismisses error sheet for non-authentication failures', async () => {
    mutateAsyncMock.mockResolvedValue(
      new FailResult(new UnexpectedError('Something went wrong')),
    );

    renderLoginPage();

    await userEvent.type(screen.getByLabelText('Username'), 'good.user');
    await userEvent.type(screen.getByLabelText('Password'), 'valid-pass');
    await userEvent.click(screen.getByRole('button', { name: 'Login' }));

    expect(await screen.findByText('Unexpected Error')).toBeInTheDocument();
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Dismiss' }));

    await waitFor(() => {
      expect(screen.queryByText('Unexpected Error')).not.toBeInTheDocument();
    });
  });
});
