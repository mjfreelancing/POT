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
          // Specific colors for each role to avoid confusion with status badges
          const roleColors: Record<string, string> = {
            Admin:
              'bg-purple-100 text-purple-800 ring-purple-600/20 dark:bg-purple-900/30 dark:text-purple-300 dark:ring-purple-400/30',
            Viewer:
              'bg-amber-100 text-amber-800 ring-amber-600/20 dark:bg-amber-900/30 dark:text-amber-300 dark:ring-amber-400/30',
          };

          // Return specific color for known roles, or default to gray for unknown roles
          return (
            roleColors[role] ||
            'bg-gray-100 text-gray-800 ring-gray-600/20 dark:bg-gray-800 dark:text-gray-400 dark:ring-gray-400/30'
          );
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
