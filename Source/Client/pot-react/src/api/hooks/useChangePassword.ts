import type { FailResultBase, Result } from '@/lib';

import { usePut } from './useApi';

type ChangePasswordRequest = {
  currentPassword: string;
  newPassword: string;
};

function useChangePassword() {
  const mutation = usePut<null, ChangePasswordRequest>('/auth/change-password');

  return {
    ...mutation,
    data: mutation.data as Result<null, FailResultBase>,
    changePassword: (fields: ChangePasswordRequest) =>
      mutation.mutateAsync({
        data: {
          currentPassword: fields.currentPassword,
          newPassword: fields.newPassword,
        },
      }),
  };
}

export type { ChangePasswordRequest };
export default useChangePassword;
