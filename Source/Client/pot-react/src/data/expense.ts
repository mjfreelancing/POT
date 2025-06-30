import { z } from 'zod';

import { Frequency } from '@/lib';

import { IdentitySchema } from './identity';
import { Paged } from './types';

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
  recurring: z.boolean(),
  amount: z.number(),
  accrued: z.number(),
});

const ExpenseSchema = BaseExpenseSchema.extend({
  ...IdentitySchema.shape,
  account: ExpenseAccountSchema,
});

const CreateExpenseSchema = BaseExpenseSchema.extend({
  accountRowId: z.string(),
});

const EditExpenseSchema = BaseExpenseSchema.extend({
  ...IdentitySchema.shape,
  accountRowId: z.string(),
});

const RenewExpensesSchema = z.object({
  rowIds: z.string().array(),
});

type Expense = z.infer<typeof ExpenseSchema>;
type CreateExpense = z.infer<typeof CreateExpenseSchema>;
type EditExpense = z.infer<typeof EditExpenseSchema>;
type RenewExpenses = z.infer<typeof RenewExpensesSchema>;
type PagedExpense = Paged<Expense>;

export {
  BaseExpenseSchema,
  CreateExpenseSchema,
  EditExpenseSchema,
  ExpenseAccountSchema,
  ExpenseSchema,
  RenewExpensesSchema,
};

export type {
  CreateExpense,
  EditExpense,
  Expense,
  PagedExpense,
  RenewExpenses,
};
