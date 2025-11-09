import { z } from 'zod';

// Approval status for pending user actions
const approvalStatusSchema = z.enum(['Approved', 'Rejected']);
type ApprovalStatus = z.infer<typeof approvalStatusSchema>;

// Pending approval user schema based on GET /api/approvals/pending response
const pendingApprovalUserSchema = z.object({
  username: z.string(),
  email: z.string().email(),
  rowId: z.string().uuid(),
  etag: z.bigint(),
});

type PendingApprovalUser = z.infer<typeof pendingApprovalUserSchema>;

// Pending user status update schema for PUT /api/approvals/{userId}/status
const pendingUserStatusUpdateSchema = z.object({
  etag: z.bigint(),
  status: approvalStatusSchema,
});

type PendingUserStatusUpdate = z.infer<typeof pendingUserStatusUpdateSchema>;

export {
  approvalStatusSchema,
  pendingApprovalUserSchema,
  pendingUserStatusUpdateSchema,
};

export type { ApprovalStatus, PendingApprovalUser, PendingUserStatusUpdate };
