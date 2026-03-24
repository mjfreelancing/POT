import { z } from 'zod';

import { EtagSchema, IdentitySchema } from './identity';

const BaseAccountSchema = z.object({
  bsb: z.string(),
  number: z.string(),
  description: z.string(),
  balance: z.number(),
  reserved: z.number(),
});

const AccountSchema = BaseAccountSchema.extend({
  ...IdentitySchema.shape,
  totalExpenseAccrued: z.number(),
  dailyExpenseAccrual: z.number(),
  stableExpenseAccrual: z.number(),
  available: z.number(),
  linkedExpenses: z.number(),
  linkedIncomes: z.number(),
});

const CreateAccountSchema = BaseAccountSchema;

const EditAccountSchema = BaseAccountSchema.extend({
  ...EtagSchema.shape,
});

type Account = z.infer<typeof AccountSchema>;
type CreateAccount = z.infer<typeof CreateAccountSchema>;
type EditAccount = z.infer<typeof EditAccountSchema>;

const EMPTY_ACCOUNT_ARRAY: Account[] = [];

const compareAccountBsbNumber = (lhs: Account, rhs: Account): number => {
  const bsbCompare = lhs.bsb.localeCompare(rhs.bsb, 'en', {
    sensitivity: 'base',
  });

  return bsbCompare !== 0
    ? bsbCompare
    : lhs.number.localeCompare(rhs.number, 'en', { sensitivity: 'base' });
};

export {
  AccountSchema,
  BaseAccountSchema,
  compareAccountBsbNumber,
  CreateAccountSchema,
  EditAccountSchema,
  EMPTY_ACCOUNT_ARRAY,
};

export type { Account, CreateAccount, EditAccount };
