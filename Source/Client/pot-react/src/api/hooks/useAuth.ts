import { usePost } from '@/api/hooks/useApi';
import type { LoginCredentials, LoginResponse } from '@/api/types/auth';
import type { FailResultBase, Result } from '@/lib';

function useLogin() {
  const mutation = usePost<LoginResponse, LoginCredentials>('/auth/login');

  // Return mutation with data typed as Result<LoginResponse, FailResultBase>
  return {
    ...mutation,
    data: mutation.data as Result<LoginResponse, FailResultBase>,
  };
}

function useLogout() {
  const mutation = usePost<void, void>('/auth/logout');

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
}

export { useLogin, useLogout };
