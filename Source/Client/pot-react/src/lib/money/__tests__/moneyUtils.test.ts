import { faker } from '@faker-js/faker';
import { describe, expect, test } from 'vitest';

import { MoneyValue } from '../../valueTypes';
import { formatMoneyValue } from '../moneyUtils';

describe('Money Utils', () => {
  describe('formatMoneyValue', () => {
    test('should format positive money value with default AUD currency and en-AU locale', () => {
      const value: MoneyValue = faker.number.float({
        min: 0,
        max: 10000,
        fractionDigits: 2,
      });
      const result = formatMoneyValue(value);

      // Matches positive money values: starts with $, followed by 1-3 digits,
      // optional groups of 3 digits with commas, and exactly 2 decimal places
      expect(result).toMatch(/^\$\d{1,3}(,\d{3})*\.\d{2}$/);

      // Uses a regex to remove currency symbol and commas before converting to number
      const numericValue = Number(result.replace(/[$,]/g, ''));
      expect(numericValue).toBe(value);
    });

    test('should format negative money value with default AUD currency and en-AU locale', () => {
      const value: MoneyValue = faker.number.float({
        min: -10000,
        max: 0,
        fractionDigits: 2,
      });
      const result = formatMoneyValue(value);

      // Matches negative money values: starts with minus sign, then $, followed by 1-3 digits,
      // optional groups of 3 digits with commas, and exactly 2 decimal places
      expect(result).toMatch(/^-\$\d{1,3}(,\d{3})*\.\d{2}$/);

      // Uses a regex to remove currency symbol, minus sign and commas before converting to number
      const numericValue = Number(result.replace(/[-$,]/g, '')) * -1;
      expect(numericValue).toBe(value);
    });

    test('should format zero value correctly with defaults', () => {
      const result = formatMoneyValue(0);
      expect(result).toBe('$0.00');
    });

    test('should respect different currency and locale combinations', () => {
      const value = 1234567.89;

      // Australian dollar variations
      expect(formatMoneyValue(value)).toBe('$1,234,567.89'); // defaults
      expect(formatMoneyValue(value, 'AUD')).toBe('$1,234,567.89'); // explicit currency
      expect(formatMoneyValue(value, 'AUD', 'en-AU')).toBe('$1,234,567.89'); // explicit locale

      // US dollar with US locale
      expect(formatMoneyValue(value, 'USD', 'en-US')).toBe('$1,234,567.89');

      // Euro with Irish locale
      expect(formatMoneyValue(value, 'EUR', 'en-IE')).toBe('€1,234,567.89');

      // Euro with German locale (uses different separators)
      // Note the non-breaking space character
      expect(formatMoneyValue(value, 'EUR', 'de-DE')).toBe(
        '1.234.567,89\u00A0€',
      );
    });
  });
});
