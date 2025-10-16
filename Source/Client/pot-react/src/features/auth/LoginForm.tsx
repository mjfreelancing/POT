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

type LoginFormProps = Omit<ComponentPropsWithoutRef<'div'>, 'onSubmit'> & {
  className?: string;
  onSubmit?: (data: LoginCredentials) => Promise<void>;
  error?: string;
  onPasswordResetError: (error: DisplayError | null) => void;
};

function LoginForm({
  className,
  onSubmit,
  error,
  onPasswordResetError,
  ...props
}: LoginFormProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showPasswordReset, setShowPasswordReset] = useState(false);

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
          <form onSubmit={handleSubmit}>
            <div className="flex flex-col mt-4 gap-6">
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
              <div className="grid mb-4 gap-3">
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
              <a
                href="#"
                tabIndex={-1}
                className="underline underline-offset-4"
              >
                Sign up
              </a>
            </div>
          </form>
        </CardContent>
      </Card>

      <PasswordResetDialog
        open={showPasswordReset}
        onOpenChange={setShowPasswordReset}
        onError={onPasswordResetError}
      />
    </div>
  );
}

export default LoginForm;
