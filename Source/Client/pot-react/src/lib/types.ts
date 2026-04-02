import type { Permission } from '@/concerns';

type MoneyValue = number;

// Define the frequency enum values (keys) for API communication
const Frequency = {
  Days: 'Days',
  Weeks: 'Weeks',
  Months: 'Months',
  EndOfMonth: 'EndOfMonth',
  Years: 'Years',
  OneTime: 'OneTime',
} as const;

// Define frequency display values for UI. Using computed property names, instead of, for example,
//   Days: 'Days'
// which would lose the direct connection to the Frequency object values, making maintenance more error-prone.
const FrequencyDisplay = {
  [Frequency.Days]: 'Days',
  [Frequency.Weeks]: 'Weeks',
  [Frequency.Months]: 'Months',
  [Frequency.EndOfMonth]: 'End of Month',
  [Frequency.Years]: 'Years',
  [Frequency.OneTime]: 'One Time',
} as const;

// The union type of allowable frequency strings (using the API values)
type Frequency = (typeof Frequency)[keyof typeof Frequency];

// An array of frequency options for dropdowns with their display values
const FrequencyOptions = Object.values(Frequency).map(value => ({
  value,
  label: FrequencyDisplay[value],
}));

// Define the renewal mode enum values for API communication
const RenewalMode = {
  Overdue: 'Overdue',
  Future: 'Future',
} as const;

// The union type of allowable renewal mode strings (using the API values)
type RenewalMode = (typeof RenewalMode)[keyof typeof RenewalMode];

type DisplayError = {
  title: string;
  description: string;
};

type ActionResultSuccess = {
  success: true;
};

type ActionResultFail = {
  success: false;
  error: DisplayError;
};

type BulkActionResult = ActionResultSuccess | ActionResultFail;

// Using this to avoid warning 'The 'permissions' logical expression could make the dependencies of useCallback Hook change on every render.'
const EMPTY_STRING_ARRAY: string[] = [];
const EMPTY_PERMISSION_ARRAY: Permission[] = [];

export {
  EMPTY_PERMISSION_ARRAY,
  EMPTY_STRING_ARRAY,
  Frequency,
  FrequencyDisplay,
  FrequencyOptions,
  RenewalMode,
};

export type {
  ActionResultFail,
  ActionResultSuccess,
  BulkActionResult,
  DisplayError,
  Frequency as FrequencyType,
  MoneyValue,
  RenewalMode as RenewalModeType,
};
