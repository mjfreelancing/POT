import { z } from 'zod';

// Chart type for displaying metrics
type ChartType = 'line' | 'bar';

// Projection metric types for chart display
type ProjectionMetric =
  | 'balance'
  | 'available'
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
    label: 'Available Amounts',
    shortLabel: 'Available',
    chartType: 'line',
    description: 'Available amount trends over time',
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

const DateBalanceSchema = z.object({
  date: z.string(), // ISO date string
  balance: z.number(),
  available: z.number(),
  incomeReceived: z.number(),
  expensesPaid: z.number(),
});

type DateBalance = z.infer<typeof DateBalanceSchema>;

const AccountDailyBalancesSchema = z.object({
  rowId: z.string(), // Unique identifier for the account
  description: z.string(), // Description of the account
  dates: z.array(DateBalanceSchema), // Array of date balances
});

type AccountDailyBalances = z.infer<typeof AccountDailyBalancesSchema>;

const ProjectionSchema = z.object({
  accounts: z.array(AccountDailyBalancesSchema), // Array of accounts with their daily balances
  global: z.array(DateBalanceSchema), // Golobal daily balances
});

type Projection = z.infer<typeof ProjectionSchema>;

export {
  AccountDailyBalancesSchema,
  DateBalanceSchema,
  PROJECTION_METRICS,
  ProjectionSchema,
};
export type {
  AccountDailyBalances,
  ChartType,
  DateBalance,
  MetricConfig,
  Projection,
  ProjectionMetric,
};
