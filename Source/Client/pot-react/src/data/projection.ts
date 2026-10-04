// Chart type for displaying metrics
type ChartType = 'line' | 'bar';

// Projection metric types for chart display
type ProjectionMetric =
  'balance' | 'dailyAccrual' | 'incomeReceived' | 'expensesPaid';

// Default projection metric to use
const DEFAULT_PROJECTION_METRIC: ProjectionMetric = 'balance';

// Which standing obligations are subtracted from the plotted balance
type ProjectionInclude = {
  reserved: boolean;
  accruals: boolean;
  arrears: boolean;
};

// Arrears is on by default so a first load plots the balance net of past-due debt
const DEFAULT_PROJECTION_INCLUDE: ProjectionInclude = {
  reserved: false,
  accruals: false,
  arrears: true,
};

type MetricConfig = {
  title: string;
  filterLabel: string;
  chartType: ChartType;
};

const PROJECTION_METRICS: Record<ProjectionMetric, MetricConfig> = {
  balance: {
    title: 'Account Balances',
    filterLabel: 'Account Balances',
    chartType: 'line',
  },

  dailyAccrual: {
    title: 'Operational Accruals',
    filterLabel: 'Operational Accruals',
    chartType: 'line',
  },

  incomeReceived: {
    title: 'Future Incomes',
    filterLabel: 'Incomes',
    chartType: 'bar',
  },

  expensesPaid: {
    title: 'Future Expenses',
    filterLabel: 'Expenses',
    chartType: 'bar',
  },
} as const;

// The combined series key; every other key is a real account.
const TOTAL_SERIES_KEY = 'total';

// Period options for projections
type PeriodOption = {
  label: string;
  value: number;
};

const PROJECTION_PERIODS: readonly PeriodOption[] = [
  { label: '1 mo', value: 1 },
  { label: '2 mo', value: 2 },
  { label: '3 mo', value: 3 },
  { label: '6 mo', value: 6 },
  { label: '9 mo', value: 9 },
  { label: '12 mo', value: 12 },
] as const;

// Default projection period to use
const DEFAULT_PROJECTION_PERIOD = 6;

// API types for income/expense items
type ProjectionIncomeItem = {
  rowId: string;
  description: string;
  amount: number;
};

type ProjectionExpenseItem = {
  rowId: string;
  description: string;
  amount: number;
};

// UI types that include the account reference
type ProjectionIncomeItemWithAccount = ProjectionIncomeItem & {
  accountRowId: string;
};

type ProjectionExpenseItemWithAccount = ProjectionExpenseItem & {
  accountRowId: string;
};

type DateValues = {
  date: string; // ISO date string
  balance: number;
  reserved: number;
  arrears: number;
  unpaidAccrual: number;
  dailyAccrual: number;
  incomeReceived: number;
  expensesPaid: number;
  expenseItems: ProjectionExpenseItem[];
  incomeItems: ProjectionIncomeItem[];
};

type AccountDailyValues = {
  rowId: string; // Unique identifier for the account
  description: string; // Description of the account
  dates: DateValues[]; // Array of date balances
};

type Projection = {
  accounts: AccountDailyValues[]; // Array of accounts with their daily balances
};

// Default ordering key for projection accounts. Applied by useApiGetProjection so the
// chart legend and the detail sections present accounts consistently.
const compareProjectionAccountDescription = (
  lhs: AccountDailyValues,
  rhs: AccountDailyValues,
): number => {
  return lhs.description.localeCompare(rhs.description, 'en', {
    sensitivity: 'base',
  });
};

export {
  compareProjectionAccountDescription,
  DEFAULT_PROJECTION_INCLUDE,
  DEFAULT_PROJECTION_METRIC,
  DEFAULT_PROJECTION_PERIOD,
  PROJECTION_METRICS,
  PROJECTION_PERIODS,
  TOTAL_SERIES_KEY,
};

export type {
  AccountDailyValues,
  ChartType,
  DateValues,
  MetricConfig,
  PeriodOption,
  Projection,
  ProjectionExpenseItem,
  ProjectionExpenseItemWithAccount,
  ProjectionInclude,
  ProjectionIncomeItem,
  ProjectionIncomeItemWithAccount,
  ProjectionMetric,
};
