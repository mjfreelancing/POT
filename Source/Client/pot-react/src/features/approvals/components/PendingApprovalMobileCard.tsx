import { useQueryClient } from '@tanstack/react-query';
import { CheckCircle, ThumbsDown, ThumbsUp } from 'lucide-react';
import { toast } from 'sonner';

import { useUpdatePendingUserStatus } from '@/api/hooks';
import { SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { logger, useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { PendingApprovalUser } from '@/data/approvals';
import { cn } from '@/lib';

type PendingApprovalMobileCardProps = {
  user: PendingApprovalUser;
};

/**
 * Mobile card component for displaying pending approval user information.
 * Uses purple/indigo theme for approval workflow.
 */
function PendingApprovalMobileCard({ user }: PendingApprovalMobileCardProps) {
  const { username, email } = user;
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const updateStatusMutation = useUpdatePendingUserStatus();
  const { setError } = useErrorContext();

  const handleApprove = async () => {
    logger.info('PendingApprovalMobileCard', 'Approving user', {
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
        'PendingApprovalMobileCard',
        'Failed to approve user',
        result.error,
      );
    }
  };

  const handleReject = async () => {
    logger.info('PendingApprovalMobileCard', 'Rejecting user', {
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
        'PendingApprovalMobileCard',
        'Failed to reject user',
        result.error,
      );
    }
  };

  return (
    <>
      <Card
        className={cn(
          'transition-all duration-200 hover:shadow-md py-2 lg:py-2.5 gap-0 flex flex-col',
          'border-purple-500/30',
          'bg-purple-50/50 dark:bg-purple-950/20',
        )}
      >
        <CardContent className="px-2 lg:px-2.5 flex flex-col flex-1">
          <div className="flex flex-col h-full">
            {/* Username */}
            <div className="mb-0.5">
              <div className="font-bold text-base lg:text-lg leading-tight text-purple-700 dark:text-purple-300">
                {username}
              </div>
            </div>

            {/* Spacer to push content to bottom */}
            <div className="flex-1" />

            {/* Bottom section with divider and details */}
            <div className="space-y-1.5 lg:space-y-2">
              {/* Divider */}
              <div className="border-t border-border" />

              {/* Email */}
              <div className="flex justify-between items-center">
                <span className="text-[11px] lg:text-sm font-medium text-foreground">
                  Email:
                </span>
                <span className="text-xs lg:text-base text-foreground truncate ml-2">
                  {email}
                </span>
              </div>

              {/* Action Buttons */}
              <div className="flex gap-2 mt-3 pt-3 border-t border-border/50">
                <Button
                  variant="outline"
                  onClick={handleReject}
                  className="flex-1 gap-2 h-9 border-red-500/50 text-red-600 hover:bg-red-50 hover:text-red-700 dark:text-red-400 dark:hover:bg-red-950/20"
                >
                  <ThumbsDown className="h-4 w-4" />
                  Reject
                </Button>
                <Button
                  variant="outline"
                  onClick={handleApprove}
                  className="flex-1 gap-2 h-9 border-green-500/50 text-green-600 hover:bg-green-50 hover:text-green-700 dark:text-green-400 dark:hover:bg-green-950/20"
                >
                  <ThumbsUp className="h-4 w-4" />
                  Approve
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default PendingApprovalMobileCard;
export type { PendingApprovalMobileCardProps };
