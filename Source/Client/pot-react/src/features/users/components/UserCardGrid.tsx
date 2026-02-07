import { ErrorSheet } from '@/components/feedback';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { SiteUser } from '@/data/siteUser';

import UserMobileCard from './UserMobileCard';

type UserCardGridProps = {
  users: SiteUser[];
  onChangeRole: (user: SiteUser) => void;
  onToggleStatus?: (user: SiteUser) => void;
  onResendInvitation?: (user: SiteUser) => void;
  currentUsername?: string;
};

/**
 * Grid view for displaying users as cards on mobile devices.
 * Uses 1-column layout to accommodate text-heavy content (emails, roles, timestamps).
 */
function UserCardGrid({
  users,
  onChangeRole,
  onToggleStatus,
  onResendInvitation,
  currentUsername,
}: UserCardGridProps) {
  const { error, setError } = useErrorContext();

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <Card className="card-elevated flex flex-col flex-1 min-h-0 py-2 gap-0">
        <CardContent className="px-4 py-2 flex-1 min-h-0 overflow-auto">
          <div className="grid grid-cols-1 gap-3">
            {users.map(user => (
              <UserMobileCard
                key={user.rowId}
                user={user}
                onChangeRole={onChangeRole}
                onToggleStatus={onToggleStatus}
                onResendInvitation={onResendInvitation}
                isCurrentUser={user.username === currentUsername}
              />
            ))}
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default UserCardGrid;
export type { UserCardGridProps };
