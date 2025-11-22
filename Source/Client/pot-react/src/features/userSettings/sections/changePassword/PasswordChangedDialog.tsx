import { CheckCircle } from 'lucide-react';

import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

type PasswordChangedDialogProps = {
  open: boolean;
  onLogout: () => void;
};

function PasswordChangedDialog({ open, onLogout }: PasswordChangedDialogProps) {
  return (
    <Dialog open={open} onOpenChange={() => {}}>
      <DialogContent
        className="sm:max-w-md"
        onPointerDownOutside={e => e.preventDefault()}
        onEscapeKeyDown={e => e.preventDefault()}
      >
        <DialogHeader>
          <div className="flex items-center gap-3">
            <CheckCircle className="h-6 w-6 text-green-600" />
            <DialogTitle>Password Changed Successfully</DialogTitle>
          </div>
          <DialogDescription className="pt-4 space-y-3">
            <p>Your password has been updated.</p>
            <p className="text-sm">
              For your security, you must sign in again.
            </p>
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button onClick={onLogout} className="w-full">
            Sign In with New Password
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export default PasswordChangedDialog;
