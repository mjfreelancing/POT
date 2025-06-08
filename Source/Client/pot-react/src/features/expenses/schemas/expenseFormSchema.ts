import { z } from 'zod';

import { Frequency } from '@/lib/types';

export const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(1, 'Value must be greater than zero');

export const expenseFormSchema = z.object({
  description: z.string().min(1, 'A description is required'),
  nextDue: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Date must be YYYY-MM-DD'),
  endDate: z
    .string()
    .optional()
    .refine(val => val === undefined || /^\d{4}-\d{2}-\d{2}$/.test(val), {
      message: 'Date must be YYYY-MM-DD',
    }),
  frequency: z.nativeEnum(Frequency),
  frequencyCount: z.number().min(1),
  amount: MoneyValueSchema,
  accountRowId: z.string().min(1, 'An account is required'),
});

export type ExpenseFormData = z.infer<typeof expenseFormSchema>;
