import { describe, expect, test, vi } from 'vitest';

import {
  invalidateCache,
  useCacheInvalidation,
} from '@/concerns/cache/cacheInvalidation';

type QueryClientMock = {
  invalidateQueries: ReturnType<typeof vi.fn>;
};

const createQueryClientMock = (): QueryClientMock => {
  return {
    invalidateQueries: vi.fn(),
  };
};

const extractInvalidatedKeys = (queryClientMock: QueryClientMock): string[] => {
  return queryClientMock.invalidateQueries.mock.calls.map(call => {
    return call[0].queryKey[0] as string;
  });
};

describe('invalidateCache', () => {
  test('invalidates a key and its transitive dependencies', () => {
    const queryClientMock = createQueryClientMock();

    invalidateCache(queryClientMock as never, ['expenses']);

    const invalidatedKeys = extractInvalidatedKeys(queryClientMock);

    expect(invalidatedKeys).toEqual(['expenses', 'accounts', 'projections']);
  });

  test('deduplicates dependencies for multiple keys', () => {
    const queryClientMock = createQueryClientMock();

    invalidateCache(queryClientMock as never, ['expenses', 'incomes']);

    const invalidatedKeys = extractInvalidatedKeys(queryClientMock);

    expect(invalidatedKeys).toEqual([
      'expenses',
      'incomes',
      'accounts',
      'projections',
    ]);
    expect(new Set(invalidatedKeys).size).toBe(invalidatedKeys.length);
  });

  test('invalidates only the key when it has no dependencies', () => {
    const queryClientMock = createQueryClientMock();

    invalidateCache(queryClientMock as never, ['users']);

    expect(extractInvalidatedKeys(queryClientMock)).toEqual(['users']);
  });

  test('supports empty key lists', () => {
    const queryClientMock = createQueryClientMock();

    invalidateCache(queryClientMock as never, []);

    expect(queryClientMock.invalidateQueries).not.toHaveBeenCalled();
  });
});

describe('useCacheInvalidation', () => {
  test('returns a function bound to the provided query client', () => {
    const queryClientMock = createQueryClientMock();

    const boundInvalidateCache = useCacheInvalidation(queryClientMock as never);

    boundInvalidateCache(['accounts']);

    expect(extractInvalidatedKeys(queryClientMock)).toEqual([
      'accounts',
      'projections',
    ]);
  });
});
