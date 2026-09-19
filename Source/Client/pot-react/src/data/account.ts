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

// Default ordering key for the accounts list. Applied by useApiGetAllAccounts so every
// consumer (tables, card grids, pickers) inherits the same order.
const compareAccountDescription = (lhs: Account, rhs: Account): number => {
  return lhs.description.localeCompare(rhs.description, 'en', {
    sensitivity: 'base',
  });
};

export {
  AccountSchema,
  BaseAccountSchema,
  compareAccountDescription,
  CreateAccountSchema,
  EditAccountSchema,
  EMPTY_ACCOUNT_ARRAY,
};

export type { Account, CreateAccount, EditAccount };
