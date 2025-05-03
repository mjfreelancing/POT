import { useState } from 'react';
import { useDeleteAccount } from './useDeleteAccount';
import { useErrorBoundary } from 'react-error-boundary';

export const useAccountDeletion = (accountRowId: string) => {
  const { showBoundary } = useErrorBoundary();
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { deleteAccount } = useDeleteAccount(accountRowId);

  const onDelete = async () => {
    try {
      await deleteAccount();
      setShowDeleteDialog(false);
    } catch (error) {
      showBoundary(error);
    }
  };

  return {
    showDeleteDialog,
    setShowDeleteDialog,
    onDelete,
  };
};
