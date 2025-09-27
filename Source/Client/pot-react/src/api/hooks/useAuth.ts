import { usePost } from '@/api/hooks/useApi';
import type { AuthTokens, LoginCredentials } from '@/api/types/auth';
import type { FailResultBase, Result } from '@/lib';

function useLogin() {
  const mutation = usePost<AuthTokens, LoginCredentials>('/auth/login');

  // Return mutation with data typed as Result<AuthTokens, FailResultBase>
  return {
    ...mutation,
    data: mutation.data as Result<AuthTokens, FailResultBase>,
  };
}

export { useLogin };
