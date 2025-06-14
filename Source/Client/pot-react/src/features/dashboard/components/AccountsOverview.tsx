import { ColumnDef } from '@tanstack/react-table';
import { PieChart } from 'lucide-react';

import StatusBadge from '@/components/feedback/badge/StatusBadge';
import { createMoneyValueColumn, DataTable } from '@/components/table';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Account } from '@/data';

const columns: ColumnDef<Account>[] = [
  {
    accessorKey: 'bsb_number',
    header: 'Account',
    cell: ({ row }) => {
      const { bsb, number, description } = row.original;
      return (
        <div>
          <div className="font-medium">{description}</div>
          <div className="text-sm text-muted-foreground">
            ({bsb}) {number}
          </div>
        </div>
      );
    },
  },
  createMoneyValueColumn<Account>('balance', 'Balance'),
  createMoneyValueColumn<Account>('available', 'Available'),
  {
    id: 'status',
    header: 'Status',
    cell: ({ row }) => {
      const { balance, available } = row.original;

      const getAccountStatus = () => {
        if (available < 0) return 'overdrawn';
        if (available < balance * 0.1) return 'low';
        return 'active';
      };

      const status = getAccountStatus();

      switch (status) {
        case 'overdrawn':
          return (
            <StatusBadge className="w-20" color="red">
              Overdrawn
            </StatusBadge>
          );

        case 'low':
          return (
            <StatusBadge className="w-20" color="orange">
              Low
            </StatusBadge>
          );

        case 'active':
          return (
            <StatusBadge className="w-20" color="green">
              Healthy
            </StatusBadge>
          );

        default:
          throw new Error(`Unknown account status: ${status}`);
      }
    },
  },
];

type AccountsOverviewProps = {
  accounts: Account[];
};

function AccountsOverview({ accounts }: AccountsOverviewProps) {
  return (
    <Card className="card-elevated">
      <CardHeader>
        <div className="flex items-center gap-3">
          <div className="p-2 rounded-lg bg-primary/10 text-primary">
            <PieChart className="h-5 w-5" />
          </div>
          <div>
            <CardTitle>Account Overview</CardTitle>
            <CardDescription>Your bank accounts at a glance</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent className="px-4">
        <DataTable columns={columns} data={accounts} />
      </CardContent>
    </Card>
  );
}

export default AccountsOverview;
export type { AccountsOverviewProps };
