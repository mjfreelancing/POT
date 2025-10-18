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
import { logger } from '@/lib';

function LoginPage() {
  const [authError, setAuthError] = useState<DisplayError | null>(null);
  const [otherError, setOtherError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();
  const loginMutation = useLogin();
  const { login } = useAuthContext();
  const queryClient = useQueryClient();

  const handleLogin = async (values: LoginCredentials) => {
    setAuthError(null);
    setOtherError(null);
    const controller = new AbortController();
    try {
      const result = await loginMutation.mutateAsync({
        data: values,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Auth', 'Login successful');

        // First set the tokens to enable authenticated API calls
        login(result.value);

        // Force an immediate me query to get user info and permissions
        await queryClient.invalidateQueries({ queryKey: ['me'] });

        navigate('/');
      } else {
        logger.warn('Auth', 'Login failed', result.error);

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
          onPasswordResetError={setOtherError}
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
