import { CheckCircle2 } from 'lucide-react';

import { Button } from '@/components/ui/button';

type SuccessMessageProps = {
  onComplete: () => void;
};

function SuccessMessage({ onComplete }: SuccessMessageProps) {
  return (
    <div className="space-y-6">
      <div className="flex flex-col items-center text-center space-y-4">
        <div className="rounded-full bg-green-500/10 p-3">
          <CheckCircle2 className="h-12 w-12 text-green-500" />
        </div>

        <div className="space-y-2">
          <p className="text-sm text-muted-foreground max-w-sm">
            A temporary password has been sent to your email.
          </p>
        </div>

        <div className="bg-muted/50 border border-border rounded-md p-4 space-y-3 w-full">
          <p className="text-sm font-medium text-center">Next Steps:</p>
          <ol className="text-sm text-muted-foreground space-y-2 list-decimal list-inside text-left">
            <li>Check your email for the temporary password</li>
            <li>Use the temporary password to log in</li>
            <li>
              Go to Settings → Change Password to set your preferred password
            </li>
          </ol>
        </div>
      </div>

      <Button onClick={onComplete} className="w-full">
        Return to Login
      </Button>
    </div>
  );
}

export default SuccessMessage;
