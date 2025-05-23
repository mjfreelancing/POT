import { z } from 'zod';

import { FrequencyEnumValues } from '@/lib/types';

import { IdentitySchema } from '../identity';

export const IncomeAccountSchema = z.object({
  rowId: z.string(),
  description: z.string(),
});

export const BaseIncomeSchema = z.object({
  description: z.string(),
  nextDue: z.date(),
  endDate: z.date(),
  frequency: z.enum(FrequencyEnumValues),
  frequencyCount: z.number(),
  amount: z.number(),
});

export const IncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  account: IncomeAccountSchema,
});

export const CreateIncomeSchema = BaseIncomeSchema.extend({
  account: z.object({ rowId: z.string() }),
});

export const EditIncomeSchema = BaseIncomeSchema.extend({
  ...IdentitySchema.shape,
  account: z.object({ rowId: z.string() }),
});

export const compareIncomeNextDue = (lhs: Income, rhs: Income): number => {
  return lhs.nextDue.getTime() - rhs.nextDue.getTime();
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
