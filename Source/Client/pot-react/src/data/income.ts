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

type Income = z.infer<typeof IncomeSchema>;
type CreateIncome = z.infer<typeof CreateIncomeSchema>;
type EditIncome = z.infer<typeof EditIncomeSchema>;
type PagedIncome = Paged<Income>;

export {
  BaseIncomeSchema,
  CreateIncomeSchema,
  EditIncomeSchema,
  IncomeAccountSchema,
  IncomeSchema,
};
export type { CreateIncome, EditIncome, Income, PagedIncome };
