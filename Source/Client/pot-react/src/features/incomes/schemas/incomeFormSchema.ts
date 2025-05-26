import { z } from 'zod';

import { Frequency } from '@/lib/types';

export const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(0, 'Value must be 0 or greater');

export const incomeFormSchema = z.object({
  description: z.string().min(1),
  nextDue: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Date must be YYYY-MM-DD'),
  endDate: z
    .string()
    .optional()
    .refine(val => val === undefined || /^\d{4}-\d{2}-\d{2}$/.test(val), {
      message: 'Date must be YYYY-MM-DD',
    }),
  frequency: z.nativeEnum(Frequency),
  frequencyCount: z.number().min(1),
  amount: MoneyValueSchema.min(1, 'Amount must be 1 or greater'),
  accountRowId: z.string().optional(),
});

export type IncomeFormData = z.infer<typeof incomeFormSchema>;
