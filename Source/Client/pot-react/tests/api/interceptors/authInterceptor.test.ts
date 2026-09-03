import { beforeEach, describe, expect, test, vi } from 'vitest';

const axiosMock = vi.hoisted(() => {
  const requestUse = vi.fn();
  const responseUse = vi.fn();
  const axiosFn = vi.fn();

  const axiosLike = Object.assign(axiosFn, {
    interceptors: {
      request: { use: requestUse },
      response: { use: responseUse },
    },
  });

  return {
    axiosFn,
    axiosLike,
    requestUse,
    responseUse,
  };
});

const concernsMock = vi.hoisted(() => ({
  logger: {
    error: vi.fn(),
  },
}));

vi.mock('axios', () => ({
  default: axiosMock.axiosLike,
}));

vi.mock('@/concerns', () => concernsMock);

type HeaderMap = {
  set: (name: string, value: string) => void;
  get: (name: string) => string | undefined;
};

function createHeaders(): HeaderMap {
  const store = new Map<string, string>();

  return {
    set: (name, value) => {
      store.set(name, value);
    },
    get: name => store.get(name),
  };
}

function createConfig(url = '/api/accounts') {
  return {
    url,
    headers: createHeaders(),
  };
}

async function loadInterceptors(tokenProvider: {
  getAccessToken: () => string | undefined;
  refreshTokens: () => Promise<string>;
  clearTokens: () => void;
  setAccessToken: (token: string | undefined) => void;
}) {
  vi.resetModules();

  const module = await import('@/api/interceptors/authInterceptor');

  module.setupAuthInterceptors(tokenProvider);

  const requestHandler = axiosMock.requestUse.mock.calls[0]?.[0] as (
    config: ReturnType<typeof createConfig>,
  ) => ReturnType<typeof createConfig>;
  const responseErrorHandler = axiosMock.responseUse.mock
    .calls[0]?.[1] as (error: {
    response?: { status: number };
    config?: ReturnType<typeof createConfig>;
  }) => Promise<unknown>;

  return {
    requestHandler,
    responseErrorHandler,
  };
}

function createTokenProvider(overrides?: {
  refreshTokens?: () => Promise<string>;
  getAccessToken?: () => string | undefined;
}) {
  return {
    getAccessToken: overrides?.getAccessToken ?? vi.fn(() => undefined),
    refreshTokens: overrides?.refreshTokens ?? vi.fn(async () => 'new-token'),
    clearTokens: vi.fn(),
    setAccessToken: vi.fn(),
  };
}

describe('setupAuthInterceptors', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('adds Authorization header when token exists', async () => {
    const tokenProvider = createTokenProvider({
      getAccessToken: vi.fn(() => 'existing-token'),
    });

    const { requestHandler } = await loadInterceptors(tokenProvider);

    const config = createConfig();
    requestHandler(config);

    expect(config.headers.get('Authorization')).toBe('Bearer existing-token');
  });

  test('does not add Authorization header when token is missing', async () => {
    const tokenProvider = createTokenProvider({
      getAccessToken: vi.fn(() => undefined),
    });

    const { requestHandler } = await loadInterceptors(tokenProvider);

    const config = createConfig();
    requestHandler(config);

    expect(config.headers.get('Authorization')).toBeUndefined();
  });

  test('rejects unchanged when error has no response or config', async () => {
    const tokenProvider = createTokenProvider();

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);
    const error = {
      message: 'network error',
    } as unknown as {
      response?: { status: number };
      config?: ReturnType<typeof createConfig>;
    };

    await expect(responseErrorHandler(error)).rejects.toBe(error);
  });

  test('rejects unchanged for non-401 responses', async () => {
    const tokenProvider = createTokenProvider();

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);
    const config = createConfig();
    const error = {
      response: { status: 403 },
      config,
    };

    await expect(responseErrorHandler(error)).rejects.toBe(error);
  });

  test('rejects unchanged for /auth endpoint 401 to avoid refresh loop', async () => {
    const tokenProvider = createTokenProvider();

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);
    const config = createConfig('/auth/refresh');
    const error = {
      response: { status: 401 },
      config,
    };

    await expect(responseErrorHandler(error)).rejects.toBe(error);
    expect(tokenProvider.refreshTokens).not.toHaveBeenCalled();
  });

  test('refreshes token and retries original request for 401 responses', async () => {
    const tokenProvider = createTokenProvider({
      refreshTokens: vi.fn(async () => 'refreshed-token'),
    });

    const retriedResponse = { data: { ok: true } };
    axiosMock.axiosFn.mockResolvedValueOnce(retriedResponse);

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);
    const config = createConfig('/api/accounts');
    const error = {
      response: { status: 401 },
      config,
    };

    await expect(responseErrorHandler(error)).resolves.toBe(retriedResponse);
    expect(tokenProvider.refreshTokens).toHaveBeenCalledTimes(1);
    expect(config.headers.get('Authorization')).toBe('Bearer refreshed-token');
    expect(axiosMock.axiosFn).toHaveBeenCalledWith(config);
  });

  test('queues concurrent 401 request and retries it after refresh succeeds', async () => {
    let resolveRefresh: ((token: string) => void) | undefined;

    const refreshPromise = new Promise<string>(resolve => {
      resolveRefresh = resolve;
    });

    const tokenProvider = createTokenProvider({
      refreshTokens: vi.fn(() => refreshPromise),
    });

    axiosMock.axiosFn
      .mockResolvedValueOnce({ data: { first: true } })
      .mockResolvedValueOnce({ data: { second: true } });

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);

    const firstConfig = createConfig('/api/accounts');
    const secondConfig = createConfig('/api/incomes');

    const firstRequest = responseErrorHandler({
      response: { status: 401 },
      config: firstConfig,
    });

    const secondRequest = responseErrorHandler({
      response: { status: 401 },
      config: secondConfig,
    });

    resolveRefresh?.('queued-token');

    const [firstResolved, secondResolved] = await Promise.all([
      firstRequest,
      secondRequest,
    ]);

    expect([firstResolved, secondResolved]).toEqual(
      expect.arrayContaining([
        { data: { first: true } },
        { data: { second: true } },
      ]),
    );
    expect(firstConfig.headers.get('Authorization')).toBe(
      'Bearer queued-token',
    );
    expect(secondConfig.headers.get('Authorization')).toBe(
      'Bearer queued-token',
    );
    expect(axiosMock.axiosFn).toHaveBeenCalledTimes(2);
  });

  test('normalizes refresh failure, clears tokens, and rejects queued requests', async () => {
    let rejectRefresh: ((error: unknown) => void) | undefined;

    const refreshPromise = new Promise<string>((_resolve, reject) => {
      rejectRefresh = reject;
    });

    const tokenProvider = createTokenProvider({
      refreshTokens: vi.fn(() => refreshPromise),
    });

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);

    const firstRequest = responseErrorHandler({
      response: { status: 401 },
      config: createConfig('/api/accounts'),
    });

    const queuedRequest = responseErrorHandler({
      response: { status: 401 },
      config: createConfig('/api/incomes'),
    });

    rejectRefresh?.(new Error('refresh failed'));

    await expect(firstRequest).rejects.toMatchObject({
      success: false,
      error: {
        code: 'Authentication Error',
        description: 'Token refresh failed',
        type: 'Api',
      },
    });
    await expect(queuedRequest).rejects.toMatchObject({
      success: false,
      error: {
        code: 'Authentication Error',
        description: 'Token refresh failed',
        type: 'Api',
      },
    });

    expect(tokenProvider.clearTokens).toHaveBeenCalledTimes(1);
    expect(concernsMock.logger.error).toHaveBeenCalledWith(
      'API',
      'Token refresh failed',
      expect.any(Error),
    );
  });

  test('passes through existing FailResult refresh errors without re-wrapping', async () => {
    const tokenProvider = createTokenProvider({
      refreshTokens: vi.fn(),
    });

    const { responseErrorHandler } = await loadInterceptors(tokenProvider);

    const { FailResult: RuntimeFailResult } = await import('@/lib');
    const { AuthenticationError: RuntimeAuthenticationError } =
      await import('@/api/errors/apiErrors');
    const failResult = new RuntimeFailResult(
      new RuntimeAuthenticationError('Refresh token expired'),
    );

    vi.mocked(tokenProvider.refreshTokens).mockRejectedValue(failResult);

    await expect(
      responseErrorHandler({
        response: { status: 401 },
        config: createConfig('/api/accounts'),
      }),
    ).rejects.toMatchObject({
      success: false,
      error: {
        code: 'Authentication Error',
        description: 'Refresh token expired',
        type: 'Api',
      },
    });

    expect(tokenProvider.clearTokens).toHaveBeenCalledTimes(1);
  });
});
