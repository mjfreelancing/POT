import { z } from 'zod';

// User status enum
export const userStatusSchema = z.enum(['Enabled', 'Disabled', 'Pending']);
export type UserStatus = z.infer<typeof userStatusSchema>;

// Site user schema based on actual API response from GET /api/users
// Extended with status field that will be added to the API
export const siteUserSchema = z.object({
  username: z.string(),
  displayName: z.string(),
  email: z.string().email(),
  roles: z.array(z.string()), // Array of role names
  status: userStatusSchema, // Status field to be added to API
  lastLoggedInUtc: z.string().datetime().nullable(),
  rowId: z.string().uuid(),
  etag: z.bigint(),
});

export type SiteUser = z.infer<typeof siteUserSchema>;

// User invitation schema for POST /api/users/invite
export const userInvitationSchema = z.object({
  username: z
    .string()
    .min(3, 'Username must be at least 3 characters')
    .max(50, 'Username must be less than 50 characters')
    .regex(
      /^[a-zA-Z0-9._-]+$/,
      'Username can only contain letters, numbers, dots, hyphens, and underscores',
    ),
  email: z
    .string()
    .email('Please enter a valid email address')
    .max(254, 'Email must be less than 254 characters'),
  roleId: z.string().uuid('Please select a role'),
});

export type UserInvitation = z.infer<typeof userInvitationSchema>;

// The API accepts a complete replacement of the user's role assignments as an array.
// Even though the UI generally assigns a single role, the payload should be an
// array of role ids. To remove all roles, send an empty array.
// Includes etag for optimistic concurrency control.
export const userRoleUpdateSchema = z.object({
  etag: z.bigint(),
  roleIds: z.array(z.string().uuid()),
});

export type UserRoleUpdate = z.infer<typeof userRoleUpdateSchema>;

// User status update schema for PUT /api/users/{userId}/status
export const userStatusUpdateSchema = z.object({
  status: z.enum(['Enabled', 'Disabled']),
});

export type UserStatusUpdate = z.infer<typeof userStatusUpdateSchema>;
