import { useGet, usePutWithId } from '@/api/hooks/useApi';
import type {
  PendingApprovalUser,
  PendingUserStatusUpdate,
} from '@/data/approvals';
import type { FailResultBase, Result } from '@/lib';

/**
 * Hook to fetch all pending approval users (Approval status)
 * Requires platform:manage permission
 * Uses .IgnoreQueryFilters() on backend to see across all sites
 */
export function useGetPendingApprovals() {
  return useGet<PendingApprovalUser[]>('/approvals/pending', [
    'pending-approvals',
  ]);
}

/**
 * Hook to update pending user status (approve or reject)
 * Requires platform:manage permission
 */
export function useUpdatePendingUserStatus() {
  const mutation = usePutWithId<void, PendingUserStatusUpdate>(
    userId => `/approvals/${userId}/status`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
}
