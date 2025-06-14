import { z } from 'zod';

import { IdentitySchema } from './identity';

const BaseAccountSchema = z.object({
  bsb: z.string(),
  number: z.string(),
  description: z.string(),
  balance: z.number(),
  reserved: z.number(),
});

const AccountSchema = BaseAccountSchema.extend({
  ...IdentitySchema.shape,
  allocated: z.number(),
  dailyAccrual: z.number(),
  available: z.number(),
  linkedExpenses: z.number(),
  linkedIncomes: z.number(),
});

const CreateAccountSchema = BaseAccountSchema;

const EditAccountSchema = BaseAccountSchema.extend({
  ...IdentitySchema.shape,
});

type Account = z.infer<typeof AccountSchema>;
type CreateAccount = z.infer<typeof CreateAccountSchema>;
type EditAccount = z.infer<typeof EditAccountSchema>;

const compareAccountBsbNumber = (lhs: Account, rhs: Account): number => {
  const bsbCompare = lhs.bsb.localeCompare(rhs.bsb);
  return bsbCompare !== 0 ? bsbCompare : lhs.number.localeCompare(rhs.number);
};

export {
  AccountSchema,
  BaseAccountSchema,
  compareAccountBsbNumber,
  CreateAccountSchema,
  EditAccountSchema,
};
export type { Account, CreateAccount, EditAccount };
