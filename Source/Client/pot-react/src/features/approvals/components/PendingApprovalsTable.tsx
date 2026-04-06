import type { ColumnDef } from '@tanstack/react-table';

import {
  createActionsColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import type { PendingApprovalUser } from '@/data/approvals';

import { PendingApprovalActions } from './PendingApprovalActions';

type PendingApprovalsTableProps = {
  users: PendingApprovalUser[];
};

function PendingApprovalsTable({ users }: PendingApprovalsTableProps) {
  const columns: ColumnDef<PendingApprovalUser>[] = [
    {
      id: 'username',
      accessorKey: 'username',
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Username" />
      ),
      enableSorting: true,
      sortingFn: 'text',
      cell: ({ row }) => {
        return <div className="font-medium">{row.original.username}</div>;
      },
    },
    {
      id: 'email',
      accessorKey: 'email',
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Email" />
      ),
      enableSorting: true,
      sortingFn: 'text',
    },
    createActionsColumn<PendingApprovalUser>(user => (
      <PendingApprovalActions user={user} />
    )),
  ];

  // Empty state message
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
    <Card className="card-elevated flex flex-col flex-1 min-h-0">
      <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
        <DataTable
          columns={columns}
          data={users}
          getRowId={createRowIdGetter<PendingApprovalUser>()}
        />
      </CardContent>
    </Card>
  );
}

export { PendingApprovalsTable };
