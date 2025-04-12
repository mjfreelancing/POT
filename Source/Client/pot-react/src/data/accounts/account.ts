import { z } from 'zod';

import { IdentitySchema } from '../identity';

export const BaseAccountSchema = z.object({
  bsb: z.string(),
  number: z.string(),
  description: z.string(),
  balance: z.number(),
  reserved: z.number(),
});

export const AccountSchema = BaseAccountSchema.extend({
  ...IdentitySchema.shape,
  allocated: z.number(),
  dailyAccrual: z.number(),
  available: z.number(),
});

export const CreateAccountSchema = BaseAccountSchema;

export const EditAccountSchema = BaseAccountSchema.extend({
  ...IdentitySchema.shape,
});

export type Account = z.infer<typeof AccountSchema>;
export type CreateAccount = z.infer<typeof CreateAccountSchema>;
export type EditAccount = z.infer<typeof EditAccountSchema>;

export const compareAccountBsbNumber = (lhs: Account, rhs: Account): number => {
  const bsbCompare = lhs.bsb.localeCompare(rhs.bsb);
  return bsbCompare !== 0 ? bsbCompare : lhs.number.localeCompare(rhs.number);
};
