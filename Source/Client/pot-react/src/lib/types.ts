export type MoneyValue = number;

export const FrequencyEnumValues = [
  'Days',
  'Weeks',
  'Months',
  'Years',
] as const;

export type Frequency = (typeof FrequencyEnumValues)[number];
