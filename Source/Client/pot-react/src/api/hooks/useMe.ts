import { User } from '@/data/user';
import { useTokens } from '@/features/auth/TokenContext';
import { FailResultBase, Result } from '@/lib';

import { useGet } from './useApi';

export function useMe() {
  // Uses TokenContext directly to avoid circular dependency with AuthContext
  // AuthContext depends on useMe for user info, so useMe must only depend on tokens
  const { tokens } = useTokens();
  const hasValidToken = tokens ? !!tokens.accessToken : false;

  const query = useGet<User>('/me', ['me'], {
    // Critical for auth flow: This endpoint initializes user permissions
    // and must run whenever a component mounts that needs fresh auth state
    refetchOnMount: true,

    // Avoid token refresh loops by preventing auto-refresh on window focus
    refetchOnWindowFocus: false,

    // Auth failures are terminal and require user action (re-login)
    retry: false,

    // Prevent unnecessary calls when we know we don't have a token
    enabled: hasValidToken,

    // Prevents UI flicker by keeping previous auth state during refresh
    placeholderData: prev => prev,
  });

  return {
    ...query,
    data: query.data as Result<User, FailResultBase>,
  };
}
