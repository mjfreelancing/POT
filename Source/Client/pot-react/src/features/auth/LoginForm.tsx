import { Clock, Loader2 } from 'lucide-react';
import { type ComponentPropsWithoutRef, useEffect, useState } from 'react';

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
import { cn } from '@/lib';

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

const LONG_WAIT_START_MS = 5000;
const LONG_WAIT_UPDATE_MS = 5000;

function formatElapsedTime(seconds: number): string {
  const minutes = Math.floor(seconds / 60)
    .toString()
    .padStart(2, '0');
  const secondsRemainder = (seconds % 60).toString().padStart(2, '0');

  return `${minutes}:${secondsRemainder}`;
}

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
  const [isLongWait, setIsLongWait] = useState(false);
  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const [showPasswordReset, setShowPasswordReset] = useState(false);
  const [showSignup, setShowSignup] = useState(false);

  useEffect(() => {
    if (!isLoading) {
      setIsLongWait(false);
      setElapsedSeconds(0);
      return;
    }

    const startTimerId = window.setTimeout(() => {
      setIsLongWait(true);
      setElapsedSeconds(5);
    }, LONG_WAIT_START_MS);

    const updateTimerId = window.setInterval(() => {
      setElapsedSeconds(currentSeconds => {
        if (currentSeconds === 0) {
          return currentSeconds;
        }

        return currentSeconds + LONG_WAIT_UPDATE_MS / 1000;
      });
    }, LONG_WAIT_UPDATE_MS);

    return () => {
      window.clearTimeout(startTimerId);
      window.clearInterval(updateTimerId);
    };
  }, [isLoading]);

  const formattedElapsedTime = formatElapsedTime(elapsedSeconds);

  let loadingButtonText = 'Signing in...';
  let loadingLiveText = '';

  if (isLongWait) {
    if (elapsedSeconds >= 60) {
      loadingButtonText = `This is taking longer than usual (${formattedElapsedTime})`;
      loadingLiveText = `This is taking longer than usual. ${formattedElapsedTime} elapsed.`;
    } else {
      loadingButtonText = `Still signing you in (${formattedElapsedTime})`;
      loadingLiveText = `Still signing you in. ${formattedElapsedTime} elapsed.`;
    }
  }

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
                  <div aria-live="polite" className="sr-only">
                    {loadingLiveText}
                  </div>
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
                    {isLoading ? (
                      <span className="inline-flex items-center justify-center gap-2">
                        <Loader2
                          className="h-4 w-4 animate-spin"
                          aria-hidden="true"
                        />
                        {loadingButtonText}
                      </span>
                    ) : (
                      'Login'
                    )}
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
