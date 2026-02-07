import {
  Mail,
  MoreHorizontal,
  RotateCcw,
  UserCheck,
  UserX,
} from 'lucide-react';

import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { SiteUser } from '@/data/siteUser';
import { WithPermission } from '@/features/auth/components';
import { formatDateTime } from '@/lib/dateUtils';
import { cn } from '@/lib/utils';

import { UserStatusBadge } from './UserStatusBadge';

type UserMobileCardProps = {
  user: SiteUser;
  onChangeRole: (user: SiteUser) => void;
  onToggleStatus?: (user: SiteUser) => void;
  onResendInvitation?: (user: SiteUser) => void;
  isCurrentUser?: boolean;
};

/**
 * Mobile card component for displaying user information.
 * Uses neutral color palette (gray/slate).
 */
function UserMobileCard({
  user,
  onChangeRole,
  onToggleStatus,
  onResendInvitation,
  isCurrentUser = false,
}: UserMobileCardProps) {
  const { displayName, username, email, roles, status, lastLoggedInUtc } = user;

  const getRoleColor = (role: string) => {
    const roleColors: Record<string, string> = {
      Admin:
        'bg-purple-100 text-purple-800 ring-purple-600/20 dark:bg-purple-900/30 dark:text-purple-300 dark:ring-purple-400/30',
      Viewer:
        'bg-amber-100 text-amber-800 ring-amber-600/20 dark:bg-amber-900/30 dark:text-amber-300 dark:ring-amber-400/30',
    };

    return (
      roleColors[role] ||
      'bg-gray-100 text-gray-800 ring-gray-600/20 dark:bg-gray-800 dark:text-gray-400 dark:ring-gray-400/30'
    );
  };

  return (
    <Card
      className={cn(
        'transition-all duration-200 hover:shadow-md py-2 lg:py-2.5 gap-0 flex flex-col',
        'border-slate-500/30',
        'bg-slate-50/50 dark:bg-slate-950/20',
      )}
    >
      <CardContent className="px-2 lg:px-2.5 flex flex-col flex-1">
        <div className="flex flex-col h-full">
          {/* User Name and Status */}
          <div className="flex items-center justify-between mb-0.5">
            <div>
              <div className="font-bold text-base lg:text-lg leading-tight text-slate-700 dark:text-slate-300">
                {displayName}
              </div>
              <div className="text-xs lg:text-sm text-muted-foreground mt-0.5">
                {username}
              </div>
            </div>
            <UserStatusBadge status={status} />
          </div>

          {/* Spacer to push content to bottom */}
          <div className="flex-1" />

          {/* Bottom section with divider and details */}
          <div className="space-y-1.5 lg:space-y-2">
            {/* Divider */}
            <div className="border-t border-border" />

            {/* User Details */}
            <div className="space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-[11px] lg:text-sm font-medium text-foreground">
                  Email:
                </span>
                <span className="text-xs lg:text-base text-foreground truncate ml-2">
                  {email}
                </span>
              </div>

              <div className="flex flex-wrap gap-1">
                {roles.map((role: string) => (
                  <span
                    key={role}
                    className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ring-1 ring-inset min-w-[80px] justify-center ${getRoleColor(role)}`}
                  >
                    {role}
                  </span>
                ))}
              </div>

              {/* Last Login and Action Menu Row */}
              <div className="flex justify-between items-center mt-3 pt-3 border-t border-border/50">
                {/* Last Login */}
                <div className="text-[10px] lg:text-xs text-foreground/70">
                  {lastLoggedInUtc
                    ? `Last login: ${formatDateTime(lastLoggedInUtc)}`
                    : 'Never logged in'}
                </div>

                {/* Action Menu - only show if user has manage permission and not current user */}
                {!isCurrentUser && (
                  <WithPermission permissions={['user:manage']} mode="all">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button
                          variant="outline"
                          size="icon"
                          className="h-8 w-8 shrink-0"
                        >
                          <span className="sr-only">Open menu</span>
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" side="top">
                        <DropdownMenuLabel className="text-sm font-semibold">
                          Actions
                        </DropdownMenuLabel>
                        <DropdownMenuItem onClick={() => onChangeRole(user)}>
                          <RotateCcw className="mr-2 h-4 w-4" />
                          Change Role
                        </DropdownMenuItem>

                        {(status === 'Enabled' || status === 'Disabled') &&
                          onToggleStatus && (
                            <DropdownMenuItem
                              onClick={() => onToggleStatus(user)}
                            >
                              {status === 'Enabled' ? (
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

                        {status === 'Pending' && onResendInvitation && (
                          <DropdownMenuItem
                            onClick={() => onResendInvitation(user)}
                          >
                            <Mail className="mr-2 h-4 w-4" />
                            Resend Invitation
                          </DropdownMenuItem>
                        )}
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </WithPermission>
                )}
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default UserMobileCard;
export type { UserMobileCardProps };
