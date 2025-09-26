import { z } from 'zod';

import { EtagSchema, IdentitySchema } from './';

const BaseUserSchema = z.object({
  displayName: z.string(),
  email: z.string(),
});

const UserSchema = BaseUserSchema.extend({
  ...IdentitySchema.shape,
  username: z.string(),
  permissions: z.array(z.string()),
});

const CreateUserSchema = BaseUserSchema.extend({
  username: z.string(),
});

const EditUserSchema = BaseUserSchema.extend({
  ...EtagSchema.shape,
});

type User = z.infer<typeof UserSchema>;
type CreateUser = z.infer<typeof CreateUserSchema>;
type EditUser = z.infer<typeof EditUserSchema>;

export { CreateUserSchema, EditUserSchema, UserSchema };
export type { CreateUser, EditUser, User };
