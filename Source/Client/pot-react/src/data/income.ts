import { z } from 'zod';

import { Frequency } from '@/lib';

import { IdentitySchema } from './identity';
import { Paged } from './types';

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
});

const IncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  account: IncomeAccountSchema,
});

const CreateIncomeSchema = BaseIncomeSchema.extend({
  accountRowId: z.string(),
});

const EditIncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  accountRowId: z.string(),
});

const RenewIncomesSchema = z.object({
  rowIds: z.string().array(),
});

type Income = z.infer<typeof IncomeSchema>;
type CreateIncome = z.infer<typeof CreateIncomeSchema>;
type EditIncome = z.infer<typeof EditIncomeSchema>;
type RenewIncomes = z.infer<typeof RenewIncomesSchema>;
type PagedIncome = Paged<Income>;

export {
  BaseIncomeSchema,
  CreateIncomeSchema,
  EditIncomeSchema,
  IncomeAccountSchema,
  IncomeSchema,
  RenewIncomesSchema,
};

export type { CreateIncome, EditIncome, Income, PagedIncome, RenewIncomes };
