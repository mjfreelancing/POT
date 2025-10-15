import type { FailResultBase, Result } from '@/lib';

import { usePost } from './useApi';

type PasswordResetSendRequest = {
  username: string;
};

type PasswordResetSendResponse = {
  referenceCode: string;
};

function useRequestPasswordReset() {
  const mutation = usePost<PasswordResetSendResponse, PasswordResetSendRequest>(
    '/auth/password-reset/send',
  );

  return {
    ...mutation,
    data: mutation.data as Result<PasswordResetSendResponse, FailResultBase>,
    sendPasswordReset: (username: string) =>
      mutation.mutateAsync({
        data: { username },
      }),
  };
}

type PasswordResetVerifyRequest = {
  username: string;
  referenceCode: string;
  verificationCode: string;
};

type PasswordResetVerifyResponse = {
  status: 'Success' | 'InvalidCode' | 'Expired' | 'TooManyAttempts';
  message: string;
  retryMinutes?: number;
};

function useVerifyPasswordReset() {
  const mutation = usePost<
    PasswordResetVerifyResponse,
    PasswordResetVerifyRequest
  >('/auth/password-reset/verify');

  return {
    ...mutation,
    data: mutation.data as Result<PasswordResetVerifyResponse, FailResultBase>,
    verifyPasswordReset: (
      username: string,
      referenceCode: string,
      verificationCode: string,
    ) =>
      mutation.mutateAsync({
        data: { username, referenceCode, verificationCode },
      }),
  };
}

export type {
  PasswordResetSendRequest,
  PasswordResetSendResponse,
  PasswordResetVerifyRequest,
  PasswordResetVerifyResponse,
};

export { useRequestPasswordReset, useVerifyPasswordReset };
