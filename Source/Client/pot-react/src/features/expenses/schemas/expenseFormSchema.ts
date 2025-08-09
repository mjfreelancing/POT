import { z } from 'zod';

import { Frequency, isAfterDate } from '@/lib';

const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(0, 'Value must be positive');

const expenseFormSchema = z
  .object({
    excludeFromCalcs: z.boolean(),
    description: z.string().min(1, 'A description is required'),
    nextDue: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Date must be YYYY-MM-DD'),
    accrualStart: z
      .string()
      .regex(/^\d{4}-\d{2}-\d{2}$/, 'Date must be YYYY-MM-DD'),
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
    const isFrequencyValid =
      data.frequency === Frequency.OneTime
        ? data.frequencyCount === 0
        : data.frequencyCount >= 1;

    if (!isFrequencyValid) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message:
          data.frequency === Frequency.OneTime
            ? 'Must be 0'
            : 'Must be at least 1',
        path: ['frequencyCount'],
      });
    }

    // Validate date relationships if endDate is defined
    if (data.endDate) {
      // Ensure nextDue is not after endDate
      if (isAfterDate(data.nextDue, data.endDate)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Cannot be after end date',
          path: ['nextDue'],
        });
      }

      // If accrual start exists, ensure it's not after endDate
      if (data.accrualStart) {
        if (isAfterDate(data.accrualStart, data.endDate)) {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: 'Cannot be after end date',
            path: ['accrualStart'],
          });
        }
      }
    }
  });

type ExpenseFormData = z.infer<typeof expenseFormSchema>;

export { expenseFormSchema, MoneyValueSchema };
export type { ExpenseFormData };
