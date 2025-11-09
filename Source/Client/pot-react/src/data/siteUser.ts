import { z } from 'zod';

// User status enum
const userStatusSchema = z.enum(['Enabled', 'Disabled', 'Pending', 'Approval']);
type UserStatus = z.infer<typeof userStatusSchema>;

// Site user schema based on actual API response from GET /api/users
const siteUserSchema = z.object({
  username: z.string(),
  displayName: z.string(),
  email: z.string().email(),
  roles: z.array(z.string()), // Array of role names
  status: userStatusSchema,
  lastLoggedInUtc: z.string().datetime().nullable(),
  rowId: z.string().uuid(),
  etag: z.bigint(),
});

type SiteUser = z.infer<typeof siteUserSchema>;

// User invitation schema for POST /api/users/invite
// Pure data schema - form validation handled separately
const userInvitationSchema = z.object({
  username: z.string(),
  email: z.string().email(),
  roleIds: z.array(z.string().uuid()),
});

type UserInvitation = z.infer<typeof userInvitationSchema>;

// User role update schema for PUT /api/users/{userId}/roles
// The API accepts a complete replacement of the user's role assignments as an array.
// Even though the UI assigns a single role, the payload should be an array of role ids.
const userRoleUpdateSchema = z.object({
  etag: z.bigint(),
  roleIds: z.array(z.string().uuid()),
});

type UserRoleUpdate = z.infer<typeof userRoleUpdateSchema>;

// User status update schema for PUT /api/users/{userId}/status
const userStatusUpdateSchema = z.object({
  etag: z.bigint(),
  status: z.enum(['Enabled', 'Disabled']),
});

type UserStatusUpdate = z.infer<typeof userStatusUpdateSchema>;

export {
  siteUserSchema,
  userInvitationSchema,
  userRoleUpdateSchema,
  userStatusSchema,
  userStatusUpdateSchema,
};

export type {
  SiteUser,
  UserInvitation,
  UserRoleUpdate,
  UserStatus,
  UserStatusUpdate,
};
