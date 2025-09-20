import type { UserInfo } from '@/api/types/userInfo';
import { FailResultBase, Result } from '@/lib';

import { useGet } from './useApi';

export function useMe() {
  const query = useGet<UserInfo>('/auth/me', ['me'], {
    refetchOnMount: true, // Always get fresh permissions on mount
    refetchOnWindowFocus: false, // Don't spam the auth endpoint on window focus
    retry: false, // Auth failures won't be fixed by retrying
  });

  return {
    ...query,
    data: query.data as Result<UserInfo, FailResultBase>,
  };
}
