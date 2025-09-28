import { z } from 'zod';

const AccrueExpensesSchema = z.object({
  rowIds: z.string().array(),
});

type AccrueExpenses = z.infer<typeof AccrueExpensesSchema>;

export { AccrueExpensesSchema };

export type { AccrueExpenses };
