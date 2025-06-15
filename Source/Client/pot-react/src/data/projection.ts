import { z } from 'zod';

const DateBalanceSchema = z.object({
  date: z.string(), // ISO date string
  balance: z.number(), // Balance for the date
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

export { AccountDailyBalancesSchema, DateBalanceSchema, ProjectionSchema };
export type { AccountDailyBalances, DateBalance, Projection };
