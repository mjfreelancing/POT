import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router';

import { AuthenticationError } from '@/api/errors/apiErrors';
import { useLogin } from '@/api/hooks/useAuth';
import type { LoginCredentials } from '@/api/types/auth';
import { ErrorSheet } from '@/components/feedback';
import { logger, useCacheInvalidation } from '@/concerns';
import { useAuthContext } from '@/features/auth/contexts';
import LoginForm from '@/features/auth/LoginForm';
import type { DisplayError } from '@/lib';

const LOGIN_TIMEOUT_MS = 120000;

function LoginPage() {
  const [authError, setAuthError] = useState<DisplayError | null>(null);
  const [otherError, setOtherError] = useState<DisplayError | null>(null);
  const [approvalMessage, setApprovalMessage] = useState<string | null>(null);
  const navigate = useNavigate();
  const loginMutation = useLogin();
  const { login } = useAuthContext();
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);

  const handleLogin = async (values: LoginCredentials) => {
    setAuthError(null);
    setOtherError(null);
    setApprovalMessage(null);
    const controller = new AbortController();
    try {
      const result = await loginMutation.mutateAsync({
        data: values,
        signal: controller.signal,
        timeoutMs: LOGIN_TIMEOUT_MS,
      });

      if (result.success) {
        const response = result.value;

        if (response.status === 'Approval') {
          logger.info('Auth', 'Account pending approval');
          // Server always provides message for Approval status
          setApprovalMessage(response.message);
          return;
        }

        if (response.status === 'Success') {
          logger.info('Auth', 'Login successful');

          // First set the tokens to enable authenticated API calls
          // Note: refreshToken is now in HTTP-only cookie
          login(response.accessToken);

          // Force an immediate me query to get user info and permissions
          invalidateCache(['me']);

          navigate('/');
          return;
        }

        // Exhaustive check - TypeScript ensures we've handled all cases
        const _exhaustive: never = response;
        logger.error('Auth', 'Unexpected login response', _exhaustive);
      } else {
        logger.warn('Auth', 'Login request failed', result.error);

        if (result.error instanceof AuthenticationError) {
          setAuthError({
            title: result.error.code,
            description: result.error.description,
          });
        } else {
          setOtherError({
            title: result.error.code,
            description: result.error.description,
          });
        }
      }
    } finally {
      controller.abort();
    }
  };

  return (
    <div className="flex min-h-screen w-full items-center justify-center p-6 md:p-10">
      <div className="w-full max-w-sm">
        <LoginForm
          onSubmit={handleLogin}
          error={authError ? authError.description : undefined}
          approvalMessage={approvalMessage || undefined}
          onPasswordResetError={setOtherError}
          onSignupError={setOtherError}
        />
        {otherError && (
          <ErrorSheet
            title={otherError.title}
            description={otherError.description}
            onDismiss={() => setOtherError(null)}
          />
        )}
      </div>
    </div>
  );
}

export default LoginPage;
