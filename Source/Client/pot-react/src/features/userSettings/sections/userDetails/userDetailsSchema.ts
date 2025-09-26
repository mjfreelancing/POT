import * as z from 'zod';

const userDetailsSchema = z.object({
  username: z.string(),
  displayName: z.string().min(1, 'Display name is required'),
  email: z.string().email('Email is invalid'),
});

export type UserDetailsFields = z.infer<typeof userDetailsSchema>;
export { userDetailsSchema };
