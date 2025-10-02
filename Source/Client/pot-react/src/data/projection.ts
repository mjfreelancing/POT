import { z } from 'zod';

// Chart type for displaying metrics
type ChartType = 'line' | 'bar';

// Projection metric types for chart display
type ProjectionMetric =
  | 'balance'
  | 'available'
  | 'dailyAccrual'
  | 'incomeReceived'
  | 'expensesPaid';

// Default projection metric to use
const DEFAULT_PROJECTION_METRIC: ProjectionMetric = 'balance';

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

  available: {
    title: 'Available Balances',
    filterLabel: 'Available Balances',
    chartType: 'line',
  },

  dailyAccrual: {
    title: 'Daily Accruals',
    filterLabel: 'Daily Accruals',
    chartType: 'line',
  },

  incomeReceived: {
    title: 'Future Income',
    filterLabel: 'Income',
    chartType: 'bar',
  },

  expensesPaid: {
    title: 'Future Expenses',
    filterLabel: 'Expenses',
    chartType: 'bar',
  },
} as const;

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

const DateValuesSchema = z.object({
  date: z.string(), // ISO date string
  balance: z.number(),
  available: z.number(),
  dailyAccrual: z.number(),
  incomeReceived: z.number(),
  expensesPaid: z.number(),
});

type DateValues = z.infer<typeof DateValuesSchema>;

const AccountDailyValuesSchema = z.object({
  rowId: z.string(), // Unique identifier for the account
  description: z.string(), // Description of the account
  dates: z.array(DateValuesSchema), // Array of date balances
});

type AccountDailyValues = z.infer<typeof AccountDailyValuesSchema>;

const ProjectionSchema = z.object({
  accounts: z.array(AccountDailyValuesSchema), // Array of accounts with their daily balances
  global: z.array(DateValuesSchema), // Golobal daily balances
});

type Projection = z.infer<typeof ProjectionSchema>;

export {
  AccountDailyValuesSchema,
  DateValuesSchema,
  DEFAULT_PROJECTION_METRIC,
  DEFAULT_PROJECTION_PERIOD,
  PROJECTION_METRICS,
  PROJECTION_PERIODS,
  ProjectionSchema,
};

export type {
  AccountDailyValues,
  ChartType,
  DateValues,
  MetricConfig,
  PeriodOption,
  Projection,
  ProjectionMetric,
};
