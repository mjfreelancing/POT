import type { MoneyValue } from './types';

/**
 * Rounds a money value to two decimals and publishes a positive zero for anything that rounds to nothing
 *
 * A total the client computes is a sum of two-decimal values, which binary arithmetic can leave a fraction of a
 * cent away from the figure a person expects. Rounding alone is not enough: a total that lands just below zero
 * rounds to a negative zero, which is a different number to `Intl.NumberFormat`, so it renders as `-$0.00`.
 *
 * @param value The numeric value to round
 * @returns The value rounded to two decimals, with a zero result carrying no sign
 * @throws TypeError When the value is not finite
 *
 * @example
 *   roundMoneyValue(0.1 + 0.2)        // 0.3
 *   roundMoneyValue(0.3 - 0.1 - 0.2)  // 0
 */
function roundMoneyValue(value: MoneyValue): MoneyValue {
  if (!Number.isFinite(value)) {
    throw new TypeError('Money value must be a finite number');
  }

  const rounded = Math.round(value * 100) / 100;

  // Negative zero compares equal to zero, so this is what removes the sign the formatter would otherwise render.
  return rounded === 0 ? 0 : rounded;
}

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
const formatMoneyValue = (
  value: MoneyValue,
  currency = 'AUD',
  locale = 'en-AU',
): string => {
  const roundedValue = roundMoneyValue(value);

  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency: currency,
  }).format(roundedValue);
};

export { formatMoneyValue, roundMoneyValue };

