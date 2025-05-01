import { MoneyValue } from './valueTypes';

export const formatMoneyValue = (
  value: MoneyValue,
  currency = 'AUD',
): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency,
  }).format(value);
};
