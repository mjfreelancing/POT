import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { authClient } from '@/api/authClient';
import { createTokenRefreshTimer } from '@/features/auth/tokenRefreshTimer';

// Mock dependencies
vi.mock('@/api/authClient', () => ({
  authClient: {
    post: vi.fn(),
  },
}));

vi.mock('@/lib/jwt', () => ({
  calculateRefreshTime: () => 5 * 60 * 1000, // Always return 5 minutes
}));

vi.mock('@/lib/logging', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

describe('tokenRefreshTimer', () => {
  const mockAccessToken = 'mock-access-token';
  const mockNewAccessToken = 'mock-new-access-token';

  beforeEach(() => {
    // Reset timers and mocks
    vi.useFakeTimers();
    vi.clearAllMocks();

    // Mock successful token refresh
    vi.mocked(authClient.post).mockResolvedValue({
      data: { accessToken: mockNewAccessToken },
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
    vi.clearAllTimers();
    vi.useRealTimers();
  });

  it('starts the refresh timer when created', () => {
    const onRefreshSuccess = vi.fn();
    const onRefreshError = vi.fn();

    const timer = createTokenRefreshTimer({
      currentAccessToken: mockAccessToken,
      onRefreshSuccess,
      onRefreshError,
    });

    timer.start();

    // Timer should be set but not triggered yet
    expect(onRefreshSuccess).not.toHaveBeenCalled();
    expect(onRefreshError).not.toHaveBeenCalled();
  });

  it('refreshes tokens when timer triggers', async () => {
    const onRefreshSuccess = vi.fn();
    const onRefreshError = vi.fn();

    const timer = createTokenRefreshTimer({
      currentAccessToken: mockAccessToken,
      onRefreshSuccess,
      onRefreshError,
    });

    timer.start();

    // Fast forward past the refresh time
    vi.advanceTimersByTime(5 * 60 * 1000); // 5 minutes

    // Let any pending promises resolve
    await vi.runAllTimersAsync();

    // Should have called refresh
    expect(authClient.post).toHaveBeenCalledWith(
      '/auth/refresh',
      {},
      {
        headers: { Authorization: `Bearer ${mockAccessToken}` },
      },
    );

    // Should have called success callback with new accessToken string
    expect(onRefreshSuccess).toHaveBeenCalledWith(mockNewAccessToken);
    expect(onRefreshError).not.toHaveBeenCalled();
  });

  it('calls error callback when refresh fails', async () => {
    const error = new Error('Refresh failed');
    vi.mocked(authClient.post).mockRejectedValueOnce(error);

    const onRefreshSuccess = vi.fn();
    const onRefreshError = vi.fn();

    const timer = createTokenRefreshTimer({
      currentAccessToken: mockAccessToken,
      onRefreshSuccess,
      onRefreshError,
    });

    timer.start();

    // Fast forward past the refresh time
    vi.advanceTimersByTime(5 * 60 * 1000); // 5 minutes

    // Let any pending promises resolve
    await vi.runAllTimersAsync();

    // Should have attempted refresh
    expect(authClient.post).toHaveBeenCalled();

    // Should have called error callback with FailResult wrapper
    expect(onRefreshError).toHaveBeenCalled();
    const errorArg = vi.mocked(onRefreshError).mock.calls[0][0];
    expect(errorArg).toHaveProperty('success', false);
    expect(errorArg).toHaveProperty('error');
    expect(onRefreshSuccess).not.toHaveBeenCalled();
  });

  it('stops the timer when requested', async () => {
    const onRefreshSuccess = vi.fn();
    const onRefreshError = vi.fn();

    const timer = createTokenRefreshTimer({
      currentAccessToken: mockAccessToken,
      onRefreshSuccess,
      onRefreshError,
    });

    timer.start();
    timer.stop();

    // Fast forward past the refresh time
    vi.advanceTimersByTime(5 * 60 * 1000); // 5 minutes

    // Let any pending promises resolve
    await vi.runAllTimersAsync();

    // Should not have attempted refresh
    expect(authClient.post).not.toHaveBeenCalled();
    expect(onRefreshSuccess).not.toHaveBeenCalled();
    expect(onRefreshError).not.toHaveBeenCalled();
  });

  it('can be restarted after stopping', async () => {
    const onRefreshSuccess = vi.fn();
    const onRefreshError = vi.fn();

    const timer = createTokenRefreshTimer({
      currentAccessToken: mockAccessToken,
      onRefreshSuccess,
      onRefreshError,
    });

    // Start, stop, then start again
    timer.start();
    timer.stop();
    timer.start();

    // Fast forward past the refresh time
    vi.advanceTimersByTime(5 * 60 * 1000); // 5 minutes

    // Let any pending promises resolve
    await vi.runAllTimersAsync();

    // Should have attempted refresh
    expect(authClient.post).toHaveBeenCalled();
    expect(onRefreshSuccess).toHaveBeenCalledWith(mockNewAccessToken);
    expect(onRefreshError).not.toHaveBeenCalled();
  });

  it('cleans up previous timer when started multiple times', async () => {
    const onRefreshSuccess = vi.fn();
    const onRefreshError = vi.fn();

    const timer = createTokenRefreshTimer({
      currentAccessToken: mockAccessToken,
      onRefreshSuccess,
      onRefreshError,
    });

    // Start multiple times
    timer.start();
    timer.start(); // Should clear previous timer
    timer.start(); // Should clear previous timer

    // Fast forward past the refresh time
    vi.advanceTimersByTime(5 * 60 * 1000); // 5 minutes

    // Let any pending promises resolve
    await vi.runAllTimersAsync();

    // Should have attempted refresh exactly once
    expect(authClient.post).toHaveBeenCalledTimes(1);
    expect(onRefreshSuccess).toHaveBeenCalledTimes(1);
    expect(onRefreshError).not.toHaveBeenCalled();
  });
});
