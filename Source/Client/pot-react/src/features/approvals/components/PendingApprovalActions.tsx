import { useQueryClient } from '@tanstack/react-query';
import {
  CheckCircle,
  MoreHorizontal,
  ThumbsDown,
  ThumbsUp,
} from 'lucide-react';
import { toast } from 'sonner';

import { useUpdatePendingUserStatus } from '@/api/hooks/useApprovals';
import { ErrorSheet } from '@/components/feedback';
import { SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useCacheInvalidation } from '@/concerns';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { PendingApprovalUser } from '@/data/approvals';

type PendingApprovalActionsProps = {
  user: PendingApprovalUser;
};

function PendingApprovalActions({ user }: PendingApprovalActionsProps) {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const updateStatusMutation = useUpdatePendingUserStatus();
  const { error, setError } = useErrorContext();

  const handleApprove = async () => {
    logger.info('PendingApprovalActions', 'Approving user', {
      userId: user.rowId,
      username: user.username,
    });

    const result = await updateStatusMutation.mutateAsync({
      id: user.rowId,
      data: {
        etag: user.etag,
        status: 'Approved',
      },
    });

    if (result.success) {
      // Invalidate pending-approvals cache
      invalidateCache(['pending-approvals']);

      toast(
        <SuccessToast
          icon={CheckCircle}
          title="User Approved"
          description={`${user.username} has been approved.`}
        />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error(
        'PendingApprovalActions',
        'Failed to approve user',
        result.error,
      );
    }
  };

  const handleReject = async () => {
    logger.info('PendingApprovalActions', 'Rejecting user', {
      userId: user.rowId,
      username: user.username,
    });

    const result = await updateStatusMutation.mutateAsync({
      id: user.rowId,
      data: {
        etag: user.etag,
        status: 'Rejected',
      },
    });

    if (result.success) {
      // Invalidate pending-approvals cache
      invalidateCache(['pending-approvals']);

      toast(
        <SuccessToast
          icon={CheckCircle}
          title="User Rejected"
          description={`${user.username} has been rejected.`}
        />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error(
        'PendingApprovalActions',
        'Failed to reject user',
        result.error,
      );
    }
  };

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Open menu</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuLabel className="text-sm font-semibold">
            Actions
          </DropdownMenuLabel>
          <DropdownMenuItem onClick={handleApprove}>
            <ThumbsUp className="mr-2 h-4 w-4" />
            Approve
          </DropdownMenuItem>
          <DropdownMenuItem onClick={handleReject}>
            <ThumbsDown className="mr-2 h-4 w-4" />
            Reject
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  );
}

export { PendingApprovalActions };
export type { PendingApprovalActionsProps };
