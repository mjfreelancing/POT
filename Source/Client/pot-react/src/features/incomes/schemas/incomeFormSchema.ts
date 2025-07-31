import { z } from 'zod';

import { Frequency } from '@/lib';

const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(0, 'Value must be positive');

const incomeFormSchema = z
  .object({
    description: z.string().min(1, 'A description is required'),
    nextDue: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Date must be YYYY-MM-DD'),
    endDate: z
      .string()
      .optional()
      .refine(val => val === undefined || /^\d{4}-\d{2}-\d{2}$/.test(val), {
        message: 'Date must be YYYY-MM-DD',
      }),
    frequency: z.nativeEnum(Frequency),
    frequencyCount: z.number(),
    amount: MoneyValueSchema,
    note: z
      .string()
      .nullable()
      .transform(val => (val === '' ? null : val)),
    accountRowId: z.string().min(1, 'An account is required'),
  })
  .superRefine((data, ctx) => {
    const isValid =
      data.frequency === Frequency.OneTime
        ? data.frequencyCount === 0
        : data.frequencyCount >= 1;

    if (!isValid) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message:
          data.frequency === Frequency.OneTime
            ? 'Must be 0'
            : 'Must be at least 1',
        path: ['frequencyCount'],
      });
    }
  });

type IncomeFormData = z.infer<typeof incomeFormSchema>;

export { incomeFormSchema, MoneyValueSchema };
export type { IncomeFormData };
