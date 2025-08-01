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

type MetricConfig = {
  label: string;
  shortLabel: string;
  chartType: ChartType;
  description?: string;
};

const PROJECTION_METRICS: Record<ProjectionMetric, MetricConfig> = {
  balance: {
    label: 'Account Balances',
    shortLabel: 'Balance',
    chartType: 'line',
    description: 'Account balance trends over time',
  },

  available: {
    label: 'Available Balances',
    shortLabel: 'Available',
    chartType: 'line',
    description: 'Available balance trends over time',
  },

  dailyAccrual: {
    label: 'Daily Accruals',
    shortLabel: 'Accruals',
    chartType: 'line',
    description: 'Daily accrual amounts over time',
  },

  incomeReceived: {
    label: 'Income Received',
    shortLabel: 'Income',
    chartType: 'bar',
    description: 'Income received by date',
  },

  expensesPaid: {
    label: 'Expenses Paid',
    shortLabel: 'Expenses',
    chartType: 'bar',
    description: 'Expenses paid by date',
  },
} as const;

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
  PROJECTION_METRICS,
  ProjectionSchema,
};

export type {
  AccountDailyValues,
  ChartType,
  DateValues,
  MetricConfig,
  Projection,
  ProjectionMetric,
};
