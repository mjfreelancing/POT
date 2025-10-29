import { Mail, MoreHorizontal, RotateCcw } from 'lucide-react';

import { useResendInvitation } from '@/api/hooks/useUsers';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { SiteUser } from '@/data/siteUser';
import useAuthContext from '@/features/auth/AuthContext';
import { usePermissions } from '@/hooks';

type UserActionsProps = {
  user: SiteUser;
  onChangeRole: (user: SiteUser) => void;
};

function UserActions({ user, onChangeRole }: UserActionsProps) {
  const resendInvitationMutation = useResendInvitation();
  const { hasPermission } = usePermissions();
  const { userInfo } = useAuthContext();
  const canManageUsers = hasPermission('user:manage');

  const handleResendInvitation = () => {
    resendInvitationMutation.mutate(user.rowId);
  };

  // Don't show actions for the current user to prevent self-lockout
  const isCurrentUser = userInfo?.username === user.username;

  if (!canManageUsers || isCurrentUser) {
    return null;
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" className="h-8 w-8 p-0">
          <span className="sr-only">Open menu</span>
          <MoreHorizontal className="h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => onChangeRole(user)}>
          <RotateCcw className="mr-2 h-4 w-4" />
          Change Role
        </DropdownMenuItem>
        {user.status === 'Pending' && (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={handleResendInvitation}>
              <Mail className="mr-2 h-4 w-4" />
              Resend Invitation
            </DropdownMenuItem>
          </>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export default UserActions;
export type { UserActionsProps };
