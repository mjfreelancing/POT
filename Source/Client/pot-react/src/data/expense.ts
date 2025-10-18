import { z } from 'zod';

import { Frequency } from '@/lib';

import { EtagSchema, IdentitySchema } from './identity';
import type { Paged } from './types';

const ExpenseAccountSchema = z.object({
  rowId: z.string(),
  description: z.string(),
});

const BaseExpenseSchema = z.object({
  description: z.string(),
  nextDue: z.string(),
  accrualStart: z.string(),
  endDate: z.string().nullable(),
  frequency: z.nativeEnum(Frequency),
  frequencyCount: z.number(),
  amount: z.number(),
  note: z.string().nullable(),
});

const ExpenseSchema = BaseExpenseSchema.extend({
  ...IdentitySchema.shape,
  excludeFromCalcs: z.boolean(),
  account: ExpenseAccountSchema,
  accrued: z.number(),
});

const CreateExpenseSchema = BaseExpenseSchema.extend({
  accountRowId: z.string(),
});

const EditExpenseSchema = BaseExpenseSchema.extend({
  ...EtagSchema.shape,
  excludeFromCalcs: z.boolean(),
  accountRowId: z.string(),
});

const ToggleExcludeExpensesSchema = z.object({
  rowIds: z.string().array(),
});

const RenewExpensesSchema = z.object({
  rowIds: z.string().array(),
  untilDate: z.string(),
});

type Expense = z.infer<typeof ExpenseSchema>;
type CreateExpense = z.infer<typeof CreateExpenseSchema>;
type EditExpense = z.infer<typeof EditExpenseSchema>;
type ToggleExcludeExpenses = z.infer<typeof ToggleExcludeExpensesSchema>;
type RenewExpenses = z.infer<typeof RenewExpensesSchema>;
type PagedExpense = Paged<Expense>;

const EMPTY_EXPENSE_ARRAY: Expense[] = [];

export {
  BaseExpenseSchema,
  CreateExpenseSchema,
  EditExpenseSchema,
  EMPTY_EXPENSE_ARRAY,
  ExpenseAccountSchema,
  ExpenseSchema,
  RenewExpensesSchema,
  ToggleExcludeExpensesSchema,
};

export type {
  CreateExpense,
  EditExpense,
  Expense,
  PagedExpense,
  RenewExpenses,
  ToggleExcludeExpenses,
};
