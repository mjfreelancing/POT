import * as z from 'zod';

function normalizeNumberInput(value: unknown): unknown {
  if (typeof value !== 'number' || Number.isNaN(value)) {
    return undefined;
  }

  return value;
}

const budgetRemindersSchema = z.object({
  enabled: z.boolean(),
  reminderDays: z.preprocess(
    normalizeNumberInput,
    z
      .number({
        required_error: 'Reminder days is required',
        invalid_type_error: 'Reminder days is required',
      })
      .int('Reminder days must be a whole number')
      .min(0, 'Reminder days must be between 0 and 31')
      .max(31, 'Reminder days must be between 0 and 31'),
  ),
  localHourTrigger: z.preprocess(
    normalizeNumberInput,
    z
      .number({
        required_error: 'Reminder hour is required',
        invalid_type_error: 'Reminder hour is required',
      })
      .int('Reminder hour must be a whole number')
      .min(0, 'Reminder hour must be between 0 and 23')
      .max(23, 'Reminder hour must be between 0 and 23'),
  ),
});

type BudgetRemindersFields = z.infer<typeof budgetRemindersSchema>;
type BudgetRemindersFormValues = z.input<typeof budgetRemindersSchema>;

export { budgetRemindersSchema };
export type { BudgetRemindersFields, BudgetRemindersFormValues };
