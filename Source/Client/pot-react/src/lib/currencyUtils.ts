import { Currency } from './valueTypes';

export const formatCurrency = (
  value: Currency,
  currency = 'USD',
): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency,
  }).format(value);
};
