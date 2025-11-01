import { z } from 'zod';

// User invitation form schema with validation messages
export const userInvitationFormSchema = z.object({
  username: z.string().nonempty('Username must be provided'),
  email: z.string().email('Please enter a valid email address'),
  roleId: z.string().min(1, 'Please select a role'),
});

export type UserInvitationFormData = z.infer<typeof userInvitationFormSchema>;

// User role update form schema with validation messages
export const userRoleUpdateFormSchema = z.object({
  roleId: z.string().min(1, 'Please select a role'),
});

export type UserRoleUpdateFormData = z.infer<typeof userRoleUpdateFormSchema>;
