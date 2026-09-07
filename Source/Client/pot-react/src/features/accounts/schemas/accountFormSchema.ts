import { z } from 'zod';

const MoneyValueSchema = z
  .number({
    error: 'This field is required',
  })
  .min(0, 'Value must be 0 or greater');

const accountFormSchema = z.object({
  bsb: z.string().regex(/^\d{3}-\d{3}$/, 'BSB must be in the format XXX-XXX'),
  number: z.string().min(1, 'Account number is required'),
  description: z.string().min(1, 'Description is required'),
  balance: MoneyValueSchema,
  reserved: MoneyValueSchema,
});

type AccountFormData = z.infer<typeof accountFormSchema>;

export { accountFormSchema, MoneyValueSchema };
export type { AccountFormData };
