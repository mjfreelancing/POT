import type { ColumnDef } from '@tanstack/react-table';

import { ErrorSheet } from '@/components/feedback';
import {
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { SiteUser } from '@/data/siteUser';
import { formatDateTime } from '@/lib/dateUtils';

import UserActions from './UserActions';
import { UserStatusBadge } from './UserStatusBadge';

type UsersTableProps = {
  users: SiteUser[];
  onChangeRole: (user: SiteUser) => void;
};

function UsersTable({ users, onChangeRole }: UsersTableProps) {
  const { error, setError } = useErrorContext();

  const columns: ColumnDef<SiteUser>[] = [
    {
      id: 'username',
      accessorKey: 'username',
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Username" />
      ),
      enableSorting: true,
      sortingFn: 'text',
      cell: ({ row }) => {
        const user = row.original;
        return (
          <div className="font-medium">
            <div>{user.username}</div>
            {user.displayName && (
              <div className="text-sm text-muted-foreground">
                {user.displayName}
              </div>
            )}
          </div>
        );
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
    {
      id: 'roles',
      accessorKey: 'roles',
      header: 'Roles',
      cell: ({ row }) => {
        const roles = row.original.roles;

        const getRoleColor = (role: string) => {
          // You can customize these colors based on specific role names
          const colors = [
            'bg-purple-100 text-purple-800 ring-purple-600/20',
            'bg-indigo-100 text-indigo-800 ring-indigo-600/20',
            'bg-teal-100 text-teal-800 ring-teal-600/20',
            'bg-orange-100 text-orange-800 ring-orange-600/20',
          ];

          // Use role name hash to consistently assign colors
          const hash = role.split('').reduce((a, b) => a + b.charCodeAt(0), 0);
          return colors[hash % colors.length];
        };

        return (
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
        );
      },
    },
    {
      id: 'status',
      accessorKey: 'status',
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Status" />
      ),
      enableSorting: true,
      sortingFn: 'text',
      cell: ({ row }) => {
        return <UserStatusBadge status={row.original.status} />;
      },
    },
    {
      id: 'lastLogin',
      accessorKey: 'lastLoggedInUtc',
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Last Login" />
      ),
      enableSorting: true,
      sortingFn: 'datetime',
      cell: ({ row }) => {
        const lastLoggedInUtc = row.original.lastLoggedInUtc;

        if (!lastLoggedInUtc) {
          return <span className="text-sm text-muted-foreground">Never</span>;
        }

        try {
          return (
            <span className="text-sm text-muted-foreground">
              {formatDateTime(lastLoggedInUtc)}
            </span>
          );
        } catch {
          return (
            <span className="text-sm text-muted-foreground">Invalid date</span>
          );
        }
      },
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const user = row.original;
        return (
          <div className="flex justify-end">
            <UserActions user={user} onChangeRole={onChangeRole} />
          </div>
        );
      },
    },
  ];

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
          <DataTable
            columns={columns}
            data={users}
            getRowId={createRowIdGetter<SiteUser>()}
          />
        </CardContent>
      </Card>
    </>
  );
}

export { UsersTable };
export type { UsersTableProps };
