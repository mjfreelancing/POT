import type { OtpVerificationStatus } from '@/features/auth/shared/types/otpTypes';
import type { FailResultBase, Result } from '@/lib';

import { usePost } from './useApi';

type SignupSendRequest = {
  username: string;
  email: string;
};

type SignupSendResponse = {
  status: 'Success' | 'UsernameTaken';
  message: string;
  referenceCode?: string; // Only present when status === 'Success'
};

function useRequestSignup() {
  const mutation = usePost<SignupSendResponse, SignupSendRequest>(
    '/auth/signup/send',
  );

  return {
    ...mutation,
    data: mutation.data as Result<SignupSendResponse, FailResultBase>,
    sendSignup: (username: string, email: string) =>
      mutation.mutateAsync({
        data: { username, email },
      }),
  };
}

type SignupCompleteRequest = {
  username: string;
  referenceCode: string;
  verificationCode: string;
};

type SignupCompleteResponse = {
  status: OtpVerificationStatus | 'UsernameTaken'; // Union with shared OTP statuses
  message: string;
  retryMinutes?: number;
};

function useCompleteSignup() {
  const mutation = usePost<SignupCompleteResponse, SignupCompleteRequest>(
    '/auth/signup/complete',
  );

  return {
    ...mutation,
    data: mutation.data as Result<SignupCompleteResponse, FailResultBase>,
    completeSignup: (
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
  SignupCompleteRequest,
  SignupCompleteResponse,
  SignupSendRequest,
  SignupSendResponse,
};

export { useCompleteSignup, useRequestSignup };
