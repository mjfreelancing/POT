import {
  useGet,
  usePost,
  usePutWithId,
  usePutWithIdNoData,
} from '@/api/hooks/useApi';
import type { Identity } from '@/data';
import type {
  SiteUser,
  UserInvitation,
  UserRoleUpdate,
  UserStatusUpdate,
} from '@/data/siteUser';
import type { FailResultBase, Result } from '@/lib';

/**
 * Hook to fetch all site users with their roles and status
 * Requires user:view permission
 */
export function useUsers() {
  return useGet<SiteUser[]>('/users', ['users']);
}

/**
 * Hook to invite a new user with role assignment
 * Requires user:manage permission
 */
export function useInviteUser() {
  const mutation = usePost<Identity, UserInvitation>('/users/invite');

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
}

/**
 * Hook to update a user's role assignment
 * Requires user:manage permission
 */
export function useUpdateUserRole() {
  const mutation = usePutWithId<Identity, UserRoleUpdate>(
    userId => `/users/${userId}/roles`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
}

/**
 * Hook to update user status (enable/disable)
 * Requires user:manage permission
 */
export function useUpdateUserStatus() {
  const mutation = usePutWithId<void, UserStatusUpdate>(
    userId => `/users/${userId}/status`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
}

/**
 * Hook to resend invitation for pending users
 * Requires user:manage permission
 */
export function useResendInvitation() {
  const mutation = usePutWithIdNoData<void>(
    userId => `/users/${userId}/resend-invitation`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
}
