import { z } from 'zod';

// Role schema based on actual API response from GET /api/roles
export const roleSchema = z.object({
  name: z.string(),
  rowId: z.string().uuid(),
  etag: z.number(),
});

export type Role = z.infer<typeof roleSchema>;

// Schema for role selection in forms (using rowId as value)
export const roleSelectionSchema = z.object({
  roleId: z.string().uuid(),
});

export type RoleSelection = z.infer<typeof roleSelectionSchema>;
