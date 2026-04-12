import { describe, expect, test } from 'vitest';

import { getErrorMessage } from '@/lib';

describe('Error Utils', () => {
  describe('getErrorMessage', () => {
    test('should return message from Error instances', () => {
      const error = new Error('Something failed');

      const result = getErrorMessage(error);

      expect(result).toBe('Something failed');
    });

    test('should return original string when input is a string', () => {
      const result = getErrorMessage('plain error text');

      expect(result).toBe('plain error text');
    });

    test('should stringify plain objects', () => {
      const result = getErrorMessage({ reason: 'bad input' });

      expect(result).toBe('[object Object]');
    });

    test('should stringify null input', () => {
      const result = getErrorMessage(null);

      expect(result).toBe('null');
    });

    test('should stringify undefined input', () => {
      const result = getErrorMessage(undefined);

      expect(result).toBe('undefined');
    });

    test('should stringify numeric input', () => {
      const result = getErrorMessage(404);

      expect(result).toBe('404');
    });
  });
});
