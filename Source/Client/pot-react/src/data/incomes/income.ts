import { z } from 'zod';

import { Frequency } from '@/lib/types';

import { IdentitySchema } from '../identity';

export const IncomeAccountSchema = z.object({
  rowId: z.string(),
  description: z.string(),
});

export const BaseIncomeSchema = z.object({
  description: z.string(),
  nextDue: z.string(),
  endDate: z.string().nullable(),
  frequency: z.nativeEnum(Frequency),
  frequencyCount: z.number(),
  amount: z.number(),
});

export const IncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  account: IncomeAccountSchema,
});

export const CreateIncomeSchema = BaseIncomeSchema.extend({
  accountRowId: z.string(),
});

export const EditIncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  accountRowId: z.string(),
});

export const compareIncomeNextDue = (lhs: Income, rhs: Income): number => {
  // Dates are stored in the format 'YYYY-MM-DD'
  return lhs.nextDue.localeCompare(rhs.nextDue);
};

export type Income = z.infer<typeof IncomeSchema>;
export type CreateIncome = z.infer<typeof CreateIncomeSchema>;
export type EditIncome = z.infer<typeof EditIncomeSchema>;

export type Paged<T> = {
  results: T[];
  totalCount: number;
  currentToken: string | null;
  previousToken: string | null;
  nextToken: string | null;
};

export type PagedIncome = Paged<Income>;
