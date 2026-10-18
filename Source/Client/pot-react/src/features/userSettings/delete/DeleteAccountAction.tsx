import { useState } from 'react';
import { useNavigate } from 'react-router';

import { DestructiveConfirmDialog } from '@/components/dialog';
import { Button } from '@/components/ui/button';
import { useAuthContext } from '@/features/auth/contexts';
import useClearSession from '@/features/auth/hooks/useClearSession';
import { setSessionNotice } from '@/features/auth/sessionNotice';

import useDeleteOwnAccount from './hooks/useDeleteOwnAccount';

const ACCOUNT_DELETED_NOTICE = 'Your account has been deleted.';

/**
 * The account owner's own deletion action, offered to every signed-in user whatever their role.
 */
function DeleteAccountAction() {
  const { userInfo } = useAuthContext();
  const clearSession = useClearSession();
  const navigate = useNavigate();
  const { deleteOwnAccount } = useDeleteOwnAccount();

  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [dialogError, setDialogError] = useState<string | undefined>(undefined);

  const username = userInfo?.username ?? '';

  function openDialog() {
    setDialogError(undefined);
    setIsDialogOpen(true);
  }

  function closeDialog() {
    setIsDialogOpen(false);
    setDialogError(undefined);
  }

  async function handleConfirm(password: string) {
    setDialogError(undefined);
    setIsDeleting(true);

    try {
      const result = await deleteOwnAccount({ username, password });

      if (!result.success) {
        // The confirmation stays open so the refusal is read in place, and the user stays signed in.
        setDialogError(result.error.description);

        return;
      }

      // The account is gone, so the session is cleared locally without asking the server to log out.
      // The notice is recorded first: the sign-in page is reached after the session is already cleared.
      setSessionNotice(ACCOUNT_DELETED_NOTICE);
      clearSession();

      navigate('/login', { replace: true });
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <>
      <div className="flex flex-col gap-1">
        <Button
          type="button"
          variant="link"
          className="h-auto w-fit justify-start p-0 text-sm font-medium text-destructive-high-contrast underline-offset-4 hover:underline"
          onClick={openDialog}
        >
          Delete my account
        </Button>
        <p className="text-xs text-muted-foreground">
          This permanently deletes your account and cannot be undone.
        </p>
      </div>

      <DestructiveConfirmDialog
        open={isDialogOpen}
        title="Delete Your Account"
        description="This permanently deletes your account."
        confirmLabel="Delete Account"
        fields={[
          {
            id: 'confirm-own-username',
            label: 'Username',
            expectedValue: username,
            placeholder: username,
          },
        ]}
        passwordLabel="Your password"
        error={dialogError}
        busy={isDeleting}
        onConfirm={({ password }) => {
          void handleConfirm(password);
        }}
        onCancel={closeDialog}
      />
    </>
  );
}

export default DeleteAccountAction;
