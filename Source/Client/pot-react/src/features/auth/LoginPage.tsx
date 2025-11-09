import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router';

import { AuthenticationError } from '@/api/errors/apiErrors';
import { useLogin } from '@/api/hooks/useAuth';
import type { LoginCredentials } from '@/api/types/auth';
import { ErrorSheet } from '@/components/feedback';
import useAuthContext from '@/features/auth/AuthContext';
import LoginForm from '@/features/auth/LoginForm';
import type { DisplayError } from '@/lib';
import { useCacheInvalidation } from '@/lib';
import { logger } from '@/lib';

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
      });

      if (result.success) {
        const response = result.value;

        if (response.status === 'Approval') {
          logger.info('Auth', 'Account pending approval');

          setApprovalMessage(
            response.message ||
              "Your account is pending approval. You'll receive an email when your account is activated.",
          );
          return;
        }

        if (response.status === 'Success') {
          if (response.accessToken && response.refreshToken) {
            logger.info('Auth', 'Login successful');

            // First set the tokens to enable authenticated API calls
            login({
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
            });

            // Force an immediate me query to get user info and permissions
            invalidateCache(['me']);

            navigate('/');
            return;
          }

          // Success status but missing tokens - fall through to error handling
          logger.error('Auth', 'Success status but missing tokens');
        } else {
          // Exhaustive check - should never reach here
          const _exhaustive: never = response.status;
          logger.error('Auth', 'Unexpected login status', _exhaustive);
        }

        // Common error handling for unexpected success/status scenarios
        setOtherError({
          title: 'Unexpected Error',
          description:
            'An unexpected error occurred during login. Please try again.',
        });
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
