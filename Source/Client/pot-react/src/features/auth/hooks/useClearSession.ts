import { useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';

import { useAccessToken } from '@/features/auth/contexts';
import { useUserStore } from '@/stores';

/**
 * Clears the signed-in session held on this device: the access token, the cached user information and the
 * query cache.
 *
 * No request is made. Use it when the session is already gone on the server, such as after the account
 * behind it has been deleted, where asking the server to log out would fail.
 */
function useClearSession() {
  const { setAccessToken } = useAccessToken();
  const userStore = useUserStore();
  const queryClient = useQueryClient();

  return useCallback(() => {
    setAccessToken(undefined);
    userStore.clearUserInfo();

    // Clear all React Query cache to prevent data leakage between users
    queryClient.clear();
  }, [queryClient, setAccessToken, userStore]);
}

export default useClearSession;
