import { z } from 'zod';

export const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(0, 'Value must be 0 or greater');

export const incomeFormSchema = z.object({
  description: z.string().min(1),
  // nextDue, frequency, frequencyCount
  amount: MoneyValueSchema,
});

export type IncomeFormSchema = z.infer<typeof incomeFormSchema>;
