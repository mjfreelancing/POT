import { z } from 'zod';

import { Frequency } from '@/lib/types';

import { IdentitySchema } from './identity';
import { Paged } from './types';

export const ExpenseAccountSchema = z.object({
  rowId: z.string(),
  description: z.string(),
});

export const BaseExpenseSchema = z.object({
  description: z.string(),
  nextDue: z.string(),
  endDate: z.string().nullable(),
  frequency: z.nativeEnum(Frequency),
  frequencyCount: z.number(),
  amount: z.number(),
});

export const ExpenseSchema = BaseExpenseSchema.extend({
  ...IdentitySchema.shape,
  account: ExpenseAccountSchema,
});

export const CreateExpenseSchema = BaseExpenseSchema.extend({
  accountRowId: z.string(),
});

export const EditExpenseSchema = BaseExpenseSchema.extend({
  ...IdentitySchema.shape,
  accountRowId: z.string(),
});

export type Expense = z.infer<typeof ExpenseSchema>;
export type CreateExpense = z.infer<typeof CreateExpenseSchema>;
export type EditExpense = z.infer<typeof EditExpenseSchema>;

export type PagedExpense = Paged<Expense>;
