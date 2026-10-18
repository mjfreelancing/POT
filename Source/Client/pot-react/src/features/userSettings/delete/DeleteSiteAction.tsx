import { useState } from 'react';
import { useNavigate } from 'react-router';

import { DestructiveConfirmDialog } from '@/components/dialog';
import { Button } from '@/components/ui/button';
import { useAuthContext } from '@/features/auth/contexts';
import useClearSession from '@/features/auth/hooks/useClearSession';
import { setSessionNotice } from '@/features/auth/sessionNotice';

import useDeleteCurrentSite from './hooks/useDeleteCurrentSite';

const SITE_DELETED_NOTICE = 'Your site has been deleted.';

const SITE_NAME_FIELD_ID = 'confirm-site-name';
const USERNAME_FIELD_ID = 'confirm-site-username';

/**
 * The site administrator's site deletion action, the last row of the danger zone.
 *
 * Deleting the site removes every user of it, so the confirmation asks for the site name as well as the
 * caller's own details, and the caller is signed out afterwards.
 */
function DeleteSiteAction() {
  const { userInfo } = useAuthContext();
  const clearSession = useClearSession();
  const navigate = useNavigate();
  const { deleteCurrentSite } = useDeleteCurrentSite();

  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [dialogError, setDialogError] = useState<string | undefined>(undefined);

  const siteName = userInfo?.site?.name ?? '';
  const username = userInfo?.username ?? '';

  function openDialog() {
    setDialogError(undefined);
    setIsDialogOpen(true);
  }

  function closeDialog() {
    setIsDialogOpen(false);
    setDialogError(undefined);
  }

  async function handleConfirm(typedSiteName: string, password: string) {
    setDialogError(undefined);
    setIsDeleting(true);

    try {
      const result = await deleteCurrentSite({
        siteName: typedSiteName,
        username,
        password,
      });

      if (!result.success) {
        // The confirmation stays open so the refusal is read in place, and the caller stays signed in.
        setDialogError(result.error.description);

        return;
      }

      // The site and its users are gone, so the session is cleared locally without asking the server to
      // log out. The notice is recorded first: the sign-in page is reached after the session is cleared.
      setSessionNotice(SITE_DELETED_NOTICE);
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
          Delete this site
        </Button>
        <p className="text-xs text-muted-foreground">
          This permanently deletes every user, account, expense and income in
          this site.
        </p>
      </div>

      <DestructiveConfirmDialog
        open={isDialogOpen}
        title={`Delete ${siteName}`}
        description="This permanently deletes the site, including every user and all of its records. No backup is taken."
        confirmLabel="Delete Site"
        fields={[
          {
            id: SITE_NAME_FIELD_ID,
            label: 'Site name',
            expectedValue: siteName,
            placeholder: siteName,
          },
          {
            id: USERNAME_FIELD_ID,
            label: 'Username',
            expectedValue: username,
            placeholder: username,
          },
        ]}
        passwordLabel="Your password"
        error={dialogError}
        busy={isDeleting}
        onConfirm={({ fields, password }) => {
          void handleConfirm(fields[SITE_NAME_FIELD_ID] ?? '', password);
        }}
        onCancel={closeDialog}
      />
    </>
  );
}

export default DeleteSiteAction;
