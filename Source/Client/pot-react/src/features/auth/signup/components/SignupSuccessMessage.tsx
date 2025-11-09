import { CheckCircle2 } from 'lucide-react';

import { Button } from '@/components/ui/button';

type SignupSuccessMessageProps = {
  onComplete: () => void;
};

function SignupSuccessMessage({ onComplete }: SignupSuccessMessageProps) {
  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center text-center space-y-5">
        <div className="rounded-full bg-green-500/10 p-3">
          <CheckCircle2 className="h-12 w-12 text-green-500" />
        </div>

        <div className="space-y-1">
          <p className="text-base font-medium text-foreground">
            Account Created Successfully!
          </p>
          <p className="text-sm text-muted-foreground">
            Check your email for next steps
          </p>
        </div>

        <div className="bg-yellow-50 border border-yellow-600 rounded-lg p-4 w-full dark:bg-yellow-950/30 dark:border-yellow-500">
          <div className="flex items-start gap-3">
            <span className="text-lg">⏳</span>
            <div className="flex-1 text-left space-y-1">
              <p className="text-sm font-semibold text-yellow-800 dark:text-yellow-400">
                Pending Approval
              </p>
              <p className="text-sm text-yellow-700 dark:text-yellow-500 leading-relaxed">
                Your account is pending approval. You&apos;ll receive an email
                when your account is activated and you can log in.
              </p>
            </div>
          </div>
        </div>

        <div className="bg-muted/30 border border-muted-foreground/20 rounded-lg p-4 w-full">
          <div className="space-y-2.5">
            <p className="text-sm font-medium text-foreground">
              What happens next:
            </p>
            <div className="space-y-2 text-sm text-muted-foreground text-left">
              <div className="flex gap-3">
                <span className="text-primary font-semibold flex-shrink-0">
                  1.
                </span>
                <p>
                  You will receive an email with a temporary password. Keep this
                  safe as it cannot be recovered.
                </p>
              </div>
              <div className="flex gap-3">
                <span className="text-primary font-semibold flex-shrink-0">
                  2.
                </span>
                <p>
                  Once approved, you&apos;ll receive an approval email with
                  instructions to log in.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <Button onClick={onComplete} className="w-full">
        Go to Login
      </Button>
    </div>
  );
}

export default SignupSuccessMessage;
