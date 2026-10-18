import { z } from 'zod';

import { EtagSchema, IdentitySchema } from './';
import { SiteSchema } from './site';

const BaseUserSchema = z.object({
  displayName: z.string(),
  email: z.string(),
});

const UserSchema = BaseUserSchema.extend({
  ...IdentitySchema.shape,
  username: z.string(),
  permissions: z.array(z.string()),
  site: SiteSchema,
});

const CreateUserSchema = BaseUserSchema.extend({
  username: z.string(),
});

const EditUserSchema = BaseUserSchema.extend({
  ...EtagSchema.shape,
});

const DeleteUserSchema = z.object({
  password: z.string(),
});

type User = z.infer<typeof UserSchema>;
type CreateUser = z.infer<typeof CreateUserSchema>;
type EditUser = z.infer<typeof EditUserSchema>;
type DeleteUser = z.infer<typeof DeleteUserSchema>;

const compareUserDisplayName = (lhs: User, rhs: User): number => {
  return lhs.displayName.localeCompare(rhs.displayName);
};

export { compareUserDisplayName, CreateUserSchema, DeleteUserSchema, EditUserSchema, UserSchema };
export type { CreateUser, DeleteUser, EditUser, User };
