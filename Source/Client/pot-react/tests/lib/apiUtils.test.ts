import { afterEach, describe, expect, test, vi } from 'vitest';

import { sleep, withDelay } from '@/lib';

describe('API Utils', () => {
  const originalNodeEnv = process.env.NODE_ENV;

  afterEach(() => {
    process.env.NODE_ENV = originalNodeEnv;
    vi.restoreAllMocks();
  });

  describe('sleep', () => {
    test('should throw outside development mode', () => {
      process.env.NODE_ENV = 'production';

      expect(() => sleep(100)).toThrow(
        'sleep should only be used in development mode',
      );
    });

    test('should not throw in development mode', () => {
      process.env.NODE_ENV = 'development';

      expect(() => sleep(100)).not.toThrow();
    });
  });

  describe('withDelay', () => {
    test('should throw outside development mode', async () => {
      process.env.NODE_ENV = 'test';

      await expect(withDelay(async () => 'ok', 100)).rejects.toThrow(
        'withDelay should only be used in development mode',
      );
    });

    test('should execute function and return result in development mode', async () => {
      process.env.NODE_ENV = 'development';

      const fn = vi.fn(async () => 'done');

      const result = await withDelay(fn, 100);

      expect(fn).toHaveBeenCalledTimes(1);
      expect(result).toBe('done');
    });

    test('should propagate function rejection in development mode', async () => {
      process.env.NODE_ENV = 'development';

      const fn = vi.fn(async () => {
        throw new Error('request failed');
      });

      await expect(withDelay(fn, 50)).rejects.toThrow('request failed');
      expect(fn).toHaveBeenCalledTimes(1);
    });
  });
});
