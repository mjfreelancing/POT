import { afterEach, describe, expect, test, vi } from 'vitest';

import {
  buildEnvScopedKey,
  buildUserScopedKey,
  resolveStorageBackend,
} from '@/concerns/storage/storageKeyBuilder';

describe('buildUserScopedKey', () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  test('produces correct key format in development', () => {
    vi.stubEnv('MODE', 'development');

    const key = buildUserScopedKey({ userId: 'abc-123', feature: 'dashboard' });

    expect(key).toBe('pot:dev:user:abc-123:dashboard');
  });

  test('produces correct key format in production', () => {
    vi.stubEnv('MODE', 'production');

    const key = buildUserScopedKey({ userId: 'abc-123', feature: 'dashboard' });

    expect(key).toBe('pot:prod:user:abc-123:dashboard');
  });

  test('is deterministic — same inputs always produce the same key', () => {
    vi.stubEnv('MODE', 'development');

    const key1 = buildUserScopedKey({ userId: 'user-xyz', feature: 'projections' });
    const key2 = buildUserScopedKey({ userId: 'user-xyz', feature: 'projections' });

    expect(key1).toBe(key2);
  });

  test('different userIds produce different keys', () => {
    vi.stubEnv('MODE', 'development');

    const key1 = buildUserScopedKey({ userId: 'user-a', feature: 'expenses' });
    const key2 = buildUserScopedKey({ userId: 'user-b', feature: 'expenses' });

    expect(key1).not.toBe(key2);
  });

  test('different features produce different keys for the same user', () => {
    vi.stubEnv('MODE', 'development');

    const key1 = buildUserScopedKey({ userId: 'user-a', feature: 'expenses' });
    const key2 = buildUserScopedKey({ userId: 'user-a', feature: 'incomes' });

    expect(key1).not.toBe(key2);
  });
});

describe('buildEnvScopedKey', () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  test('produces correct key format in development', () => {
    vi.stubEnv('MODE', 'development');

    const key = buildEnvScopedKey('theme');

    expect(key).toBe('pot:dev:theme');
  });

  test('produces correct key format in production', () => {
    vi.stubEnv('MODE', 'production');

    const key = buildEnvScopedKey('theme');

    expect(key).toBe('pot:prod:theme');
  });

  test('is deterministic — same inputs always produce the same key', () => {
    vi.stubEnv('MODE', 'development');

    const key1 = buildEnvScopedKey('theme');
    const key2 = buildEnvScopedKey('theme');

    expect(key1).toBe(key2);
  });

  test('different features produce different keys', () => {
    vi.stubEnv('MODE', 'development');

    const key1 = buildEnvScopedKey('theme');
    const key2 = buildEnvScopedKey('other');

    expect(key1).not.toBe(key2);
  });
});

describe('resolveStorageBackend', () => {
  test('returns localStorage for persistent mode', () => {
    const backend = resolveStorageBackend('persistent');

    expect(backend).toBe(localStorage);
  });

  test('returns sessionStorage for session mode', () => {
    const backend = resolveStorageBackend('session');

    expect(backend).toBe(sessionStorage);
  });
});
