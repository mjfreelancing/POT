import { Clock } from 'lucide-react';
import { type ComponentPropsWithoutRef, useState } from 'react';

import type { LoginCredentials } from '@/api/types/auth';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';

import { PasswordResetDialog } from './passwordReset';
import type { DisplayError } from './passwordReset/types/passwordResetTypes';
import { SignupDialog } from './signup';

type LoginFormProps = Omit<ComponentPropsWithoutRef<'div'>, 'onSubmit'> & {
  className?: string;
  onSubmit?: (data: LoginCredentials) => Promise<void>;
  error?: string;
  approvalMessage?: string;
  onPasswordResetError: (error: DisplayError | null) => void;
  onSignupError: (error: DisplayError | null) => void;
};

function LoginForm({
  className,
  onSubmit,
  error,
  approvalMessage,
  onPasswordResetError,
  onSignupError,
  ...props
}: LoginFormProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showPasswordReset, setShowPasswordReset] = useState(false);
  const [showSignup, setShowSignup] = useState(false);

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();

    if (onSubmit) {
      setIsLoading(true);
      try {
        await onSubmit({ username, password });
      } finally {
        setIsLoading(false);
      }
    }
  }

  return (
    <div className={cn('flex flex-col gap-6', className)} {...props}>
      <Card>
        <CardHeader>
          <CardTitle className="text-lg font-bold tracking-tight text-primary">
            Login to your account
          </CardTitle>
          <CardDescription>
            Enter your credentials to login to your account
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {approvalMessage && (
              <div className="rounded-lg border border-yellow-600 bg-yellow-50 p-4 dark:border-yellow-500 dark:bg-yellow-950/30">
                <div className="flex items-start gap-3">
                  <Clock className="mt-0.5 h-5 w-5 flex-shrink-0 text-yellow-600 dark:text-yellow-500" />
                  <div className="flex-1">
                    <h3 className="text-base font-bold text-yellow-900 dark:text-yellow-300">
                      Account Pending Approval
                    </h3>
                    <p className="mt-1 text-sm text-yellow-700 dark:text-yellow-500">
                      {approvalMessage}
                    </p>
                  </div>
                </div>
              </div>
            )}
            <form onSubmit={handleSubmit}>
              <div className="flex flex-col gap-6">
                <div className="grid gap-3">
                  <Label htmlFor="username">Username</Label>
                  <Input
                    placeholder="Enter your username"
                    id="username"
                    name="username"
                    autoComplete="username"
                    type="text"
                    required
                    value={username}
                    onChange={e => setUsername(e.target.value.trim())}
                    tabIndex={1}
                  />
                </div>
                <div className="grid gap-3">
                  <div className="flex items-center">
                    <Label htmlFor="password">Password</Label>
                    <button
                      type="button"
                      onClick={() => setShowPasswordReset(true)}
                      className="ml-auto inline-block text-sm underline-offset-4 hover:underline text-muted-foreground hover:text-primary transition-colors"
                      tabIndex={3}
                    >
                      Forgot your password?
                    </button>
                  </div>
                  <Input
                    id="password"
                    name="password"
                    autoComplete="password"
                    type="password"
                    placeholder="Enter your password"
                    required
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    tabIndex={2}
                  />
                </div>
                {error && <div className="text-red-600 text-sm">{error}</div>}
                <div className="flex flex-col gap-3">
                  <Button
                    type="submit"
                    className="w-full"
                    disabled={
                      isLoading ||
                      username.trim() === '' ||
                      password.trim() === ''
                    }
                    tabIndex={4}
                  >
                    {isLoading ? 'Logging in...' : 'Login'}
                  </Button>
                </div>
              </div>
              <div className="mt-4 text-center text-sm">
                <span>Don&apos;t have an account?</span>
                <span className="inline-block w-2" />
                <button
                  type="button"
                  onClick={() => setShowSignup(true)}
                  className="underline underline-offset-4 text-foreground hover:text-primary transition-colors"
                  tabIndex={5}
                >
                  Sign up
                </button>
              </div>
            </form>
          </div>
        </CardContent>
      </Card>

      <PasswordResetDialog
        open={showPasswordReset}
        onOpenChange={setShowPasswordReset}
        onError={onPasswordResetError}
      />

      <SignupDialog
        open={showSignup}
        onOpenChange={setShowSignup}
        onError={onSignupError}
      />
    </div>
  );
}

export default LoginForm;
