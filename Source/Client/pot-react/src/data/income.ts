import { z } from 'zod';

import { compareDates, Frequency } from '@/lib';

import { IdentitySchema } from './identity';

const IncomeAccountSchema = z.object({
  rowId: z.string(),
  description: z.string(),
});

const BaseIncomeSchema = z.object({
  description: z.string(),
  nextDue: z.string(),
  endDate: z.string().nullable(),
  frequency: z.nativeEnum(Frequency),
  frequencyCount: z.number(),
  amount: z.number(),
  note: z.string().nullable(),
});

const IncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  excludeFromCalcs: z.boolean(),
  account: IncomeAccountSchema,
});

const CreateIncomeSchema = BaseIncomeSchema.extend({
  accountRowId: z.string(),
});

const EditIncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  excludeFromCalcs: z.boolean(),
  accountRowId: z.string(),
});

const ToggleExcludeIncomesSchema = z.object({
  rowIds: z.string().array(),
});

const RenewIncomesSchema = z.object({
  rowIds: z.string().array(),
  untilDate: z.string(),
});

type Income = z.infer<typeof IncomeSchema>;
type CreateIncome = z.infer<typeof CreateIncomeSchema>;
type EditIncome = z.infer<typeof EditIncomeSchema>;
type RenewIncomes = z.infer<typeof RenewIncomesSchema>;
type ToggleExcludeIncomes = z.infer<typeof ToggleExcludeIncomesSchema>;

const compareIncomeNextDue = (lhs: Income, rhs: Income): number => {
  return compareDates(lhs.nextDue, rhs.nextDue);
};

export {
  BaseIncomeSchema,
  compareIncomeNextDue,
  CreateIncomeSchema,
  EditIncomeSchema,
  IncomeAccountSchema,
  IncomeSchema,
  RenewIncomesSchema,
  ToggleExcludeIncomesSchema,
};

export type {
  CreateIncome,
  EditIncome,
  Income,
  RenewIncomes,
  ToggleExcludeIncomes,
};
