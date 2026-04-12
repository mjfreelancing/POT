import { afterEach, describe, expect, test } from 'vitest';

import { cn, isDevelopment, isNumber, noop } from '@/lib';

describe('Utils', () => {
  describe('cn', () => {
    test('should merge class names and remove falsy entries', () => {
      const result = cn('px-2', false && 'hidden', undefined, 'py-4');

      expect(result).toBe('px-2 py-4');
    });

    test('should resolve tailwind conflicts using last class wins', () => {
      const result = cn('p-2', 'p-4');

      expect(result).toBe('p-4');
    });

    test('should support object and array class inputs', () => {
      const result = cn(['base', { active: true, disabled: false }], 'extra');

      expect(result).toBe('base active extra');
    });
  });

  describe('isDevelopment', () => {
    const originalNodeEnv = process.env.NODE_ENV;

    afterEach(() => {
      process.env.NODE_ENV = originalNodeEnv;
    });

    test('should return true when NODE_ENV is development', () => {
      process.env.NODE_ENV = 'development';

      expect(isDevelopment()).toBe(true);
    });

    test('should return false when NODE_ENV is production', () => {
      process.env.NODE_ENV = 'production';

      expect(isDevelopment()).toBe(false);
    });
  });

  describe('noop', () => {
    test('should return undefined', () => {
      const result = noop();

      expect(result).toBeUndefined();
    });
  });

  describe('isNumber', () => {
    test('should return true for finite numbers', () => {
      expect(isNumber(0)).toBe(true);
      expect(isNumber(123.45)).toBe(true);
      expect(isNumber(-10)).toBe(true);
    });

    test('should return false for NaN', () => {
      expect(isNumber(Number.NaN)).toBe(false);
    });

    test('should return true for Infinity in current implementation', () => {
      expect(isNumber(Number.POSITIVE_INFINITY)).toBe(true);
      expect(isNumber(Number.NEGATIVE_INFINITY)).toBe(true);
    });

    test('should return false for non-number values', () => {
      expect(isNumber('123')).toBe(false);
      expect(isNumber(null)).toBe(false);
      expect(isNumber(undefined)).toBe(false);
      expect(isNumber({})).toBe(false);
    });
  });
});
