import { useQueryClient } from '@tanstack/react-query';
import {
  CheckCircle,
  Mail,
  MoreHorizontal,
  RotateCcw,
  UserCheck,
  UserX,
} from 'lucide-react';
import { toast } from 'sonner';

import { useResendInvitation, useUpdateUserStatus } from '@/api/hooks';
import { SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { logger, useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { SiteUser } from '@/data/siteUser';
import { useAuthContext } from '@/features/auth/contexts';
import { usePermissions } from '@/hooks';

type UserActionsProps = {
  user: SiteUser;
  onChangeRole: (user: SiteUser) => void;
};

function UserActions({ user, onChangeRole }: UserActionsProps) {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const resendInvitationMutation = useResendInvitation();
  const updateStatusMutation = useUpdateUserStatus();
  const { setError } = useErrorContext();
  const { hasPermission } = usePermissions();
  const { userInfo } = useAuthContext();
  const canManageUsers = hasPermission('user:manage');

  const handleResendInvitation = async () => {
    logger.info('UserActions', 'Resending invitation', {
      userId: user.rowId,
      username: user.username,
    });

    const result = await resendInvitationMutation.mutateAsync({
      id: user.rowId,
    });

    if (result.success) {
      toast(
        <SuccessToast
          icon={Mail}
          title="Invitation Resent"
          description={`Invitation has been sent to ${user.username}.`}
        />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error('UserActions', 'Failed to resend invitation', result.error);
    }
  };

  const handleToggleStatus = async () => {
    const newStatus = user.status === 'Enabled' ? 'Disabled' : 'Enabled';

    logger.info('UserActions', 'Updating user status', {
      userId: user.rowId,
      username: user.username,
      currentStatus: user.status,
      newStatus,
    });

    const result = await updateStatusMutation.mutateAsync({
      id: user.rowId,
      data: {
        etag: user.etag,
        status: newStatus,
      },
    });

    if (result.success) {
      invalidateCache(['users']);

      toast(
        <SuccessToast
          icon={CheckCircle}
          title="Status Updated"
          description={`${user.username} has been ${newStatus.toLowerCase()}.`}
        />,
      );
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error('UserActions', 'Failed to update user status', result.error);
    }
  };

  // Don't show actions for the current user to prevent self-lockout
  const isCurrentUser = userInfo?.username === user.username;

  if (!canManageUsers || isCurrentUser) {
    return null;
  }

  return (
    <>
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
          <DropdownMenuItem onClick={() => onChangeRole(user)}>
            <RotateCcw className="mr-2 h-4 w-4" />
            Change Role
          </DropdownMenuItem>

          {(user.status === 'Enabled' || user.status === 'Disabled') && (
            <DropdownMenuItem onClick={handleToggleStatus}>
              {user.status === 'Enabled' ? (
                <>
                  <UserX className="mr-2 h-4 w-4" />
                  Disable User
                </>
              ) : (
                <>
                  <UserCheck className="mr-2 h-4 w-4" />
                  Enable User
                </>
              )}
            </DropdownMenuItem>
          )}

          {user.status === 'Pending' && (
            <DropdownMenuItem onClick={handleResendInvitation}>
              <Mail className="mr-2 h-4 w-4" />
              Resend Invitation
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  );
}

export default UserActions;
export type { UserActionsProps };
