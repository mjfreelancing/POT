import { usePost } from '@/api/hooks/useApi';
import type { LoginCredentials } from '@/api/types/auth';
import type { FailResultBase, Result } from '@/lib';

type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  // TODO: Use this for proactive token refresh in the future - ? use a timestamp
  refreshTokenExpiryUtc: string;
};

function useLogin() {
  const mutation = usePost<LoginResponse, LoginCredentials>('/auth/login');

  // Return mutation with data typed as Result<LoginResponse, FailResultBase>
  return {
    ...mutation,
    data: mutation.data as Result<LoginResponse, FailResultBase>,
  };
}

export type { LoginResponse };
export default useLogin;
