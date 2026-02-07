import { Card, CardContent } from '@/components/ui/card';
import type { PendingApprovalUser } from '@/data/approvals';

import PendingApprovalMobileCard from './PendingApprovalMobileCard';

type PendingApprovalCardGridProps = {
  users: PendingApprovalUser[];
};

/**
 * Grid container for displaying pending approval users in mobile view.
 * Uses single-column layout for better readability of user information.
 */
function PendingApprovalCardGrid({ users }: PendingApprovalCardGridProps) {
  // Empty state
  if (users.length === 0) {
    return (
      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex items-center justify-center">
          <div className="text-center text-muted-foreground">
            <p className="text-lg font-medium">No pending approvals</p>
            <p className="text-sm mt-2">All user signups have been processed</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="py-2 gap-0 flex-1 min-h-0">
      <CardContent className="px-4 py-2 h-full overflow-auto">
        <div className="grid grid-cols-1 gap-3">
          {users.map(user => (
            <PendingApprovalMobileCard key={user.rowId} user={user} />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

export { PendingApprovalCardGrid };
export type { PendingApprovalCardGridProps };
