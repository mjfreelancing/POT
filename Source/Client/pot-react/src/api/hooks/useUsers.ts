import { useQueryClient } from '@tanstack/react-query';

import { useGet, usePost, usePutWithId } from '@/api/hooks/useApi';
import type { Identity } from '@/data';
import type { SiteUser, UserInvitation, UserRoleUpdate } from '@/data/siteUser';
import type { FailResultBase, Result } from '@/lib';
import { useCacheInvalidation } from '@/lib';

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
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const mutation = usePost<Identity, UserInvitation>('/users/invite');

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
    mutate: (data: UserInvitation) => {
      mutation.mutate(
        { data },
        {
          onSuccess: () => {
            invalidateCache(['users']);
          },
        },
      );
    },
  };
}

/**
 * Hook to update a user's role assignment
 * Requires user:manage permission
 */
export function useUpdateUserRole() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const mutation = usePutWithId<Identity, UserRoleUpdate>(
    userId => `/users/${userId}/roles`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
    mutate: (userId: string, data: UserRoleUpdate) => {
      mutation.mutate(
        { id: userId, data },
        {
          onSuccess: () => {
            invalidateCache(['users']);
          },
        },
      );
    },
  };
}

/**
 * Hook to resend invitation for pending users
 * Requires user:manage permission
 */
export function useResendInvitation() {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);

  // Use usePutWithId pattern for dynamic URL with userId
  const mutation = usePutWithId<void, void>(
    userId => `/users/${userId}/resend-invitation`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
    mutate: (userId: string) => {
      mutation.mutate(
        { id: userId, data: undefined as void },
        {
          onSuccess: () => {
            invalidateCache(['users']);
          },
        },
      );
    },
  };
}
