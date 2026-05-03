import { afterEach, describe, expect, test, vi } from 'vitest';

import { resolveStorageEnv } from '@/concerns/storage/storageEnv';

describe('resolveStorageEnv', () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  test('returns "prod" when MODE is production', () => {
    vi.stubEnv('MODE', 'production');

    expect(resolveStorageEnv()).toBe('prod');
  });

  test('returns "dev" when MODE is development', () => {
    vi.stubEnv('MODE', 'development');

    expect(resolveStorageEnv()).toBe('dev');
  });

  test('returns "dev" when MODE is test', () => {
    vi.stubEnv('MODE', 'test');

    expect(resolveStorageEnv()).toBe('dev');
  });

  test('returns "dev" when MODE is an unrecognised value', () => {
    vi.stubEnv('MODE', 'staging');

    expect(resolveStorageEnv()).toBe('dev');
  });
});
