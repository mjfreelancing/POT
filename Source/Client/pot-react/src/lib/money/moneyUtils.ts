import { MoneyValue } from '../types';

/**
 * Formats a numeric value as a localized currency string
 * @param value The numeric value to format
 * @param currency The ISO 4217 currency code (defaults to 'AUD')
 * @param locale The BCP 47 language tag for formatting (defaults to 'en-AU')
 * @returns A formatted currency string
 *
 * @example
 * Australian dollars with Australian English locale
 *   formatMoneyValue(1234.56)                 // '$1,234.56'
 *   formatMoneyValue(1234.56, 'AUD')          // '$1,234.56'
 *   formatMoneyValue(1234.56, 'AUD', 'en-AU') // '$1,234.56'
 *
 * US dollars with US English locale
 *   formatMoneyValue(1234.56, 'USD', 'en-US') // '$1,234.56'
 *
 * Euros with Irish English locale
 *   formatMoneyValue(1234.56, 'EUR', 'en-IE') // '€1,234.56'
 */
export const formatMoneyValue = (
  value: MoneyValue,
  currency = 'AUD',
  locale = 'en-AU',
): string => {
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency: currency,
  }).format(value);
};
