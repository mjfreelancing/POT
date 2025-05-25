export type MoneyValue = number;

// Define a const-object for frequencies to ensure type-safety and named access
export const Frequency = {
  Days: 'Days',
  Weeks: 'Weeks',
  Months: 'Months',
  Years: 'Years',
} as const;

// The union type of allowable frequency strings
export type Frequency = (typeof Frequency)[keyof typeof Frequency];

// An array of all frequency values, for e.g. dropdown options
export const FrequencyEnumValues: Frequency[] = Object.values(Frequency);
