import { useGet } from '@/api/hooks/useApi';
import type { Role } from '@/data/role';

/**
 * Hook to fetch all available roles for dropdown selection
 * Requires user:view permission
 */
export function useRoles() {
  return useGet<Role[]>('/roles', ['roles']);
}
