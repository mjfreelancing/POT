import { FrequencyEnumValues } from '@/lib/types';
import { z } from 'zod';

export const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(0, 'Value must be 0 or greater');

export const incomeFormSchema = z.object({
  description: z.string().min(1),
  nextDue: z.date(),
  endDate: z.date().nullable(),
  frequency: z.enum(FrequencyEnumValues),
  frequencyCount: z.number().min(1),
  amount: MoneyValueSchema,
  account: z.object({
    rowId: z.string(),
  }),
});

export type IncomeFormSchema = z.infer<typeof incomeFormSchema>;
