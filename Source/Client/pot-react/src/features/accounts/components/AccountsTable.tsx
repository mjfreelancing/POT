import { BanknoteArrowDown, BanknoteArrowUp } from 'lucide-react';
import { useNavigate, useParams } from 'react-router';

import { StatusBadge } from '@/components/feedback';
import type { AppColumnDef } from '@/components/table';
import {
  createActionsColumn,
  createMoneyValueColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import type { Account } from '@/data';
import useUserStore from '@/stores/useUserStore';

import persistLinkedAccountFilter from '../utils/persistLinkedAccountFilter';
import AccountActions from './AccountActions';

type AccountsTableProps = {
  accounts: Account[];
};

function AccountsTable({ accounts }: AccountsTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const userId = useUserStore(store => store.userInfo?.rowId);

  const columns: AppColumnDef<Account>[] = [
    {
      id: 'description',
      accessorKey: 'description',
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title="Description"
          hint="A description identifying the account. Badges link to the expenses and incomes recorded against the account."
        />
      ),
      enableSorting: true,
      sortFn: 'text',
      cell: ({ row }) => {
        const account = row.original;
        const hasLinkedData =
          account.linkedExpenses > 0 || account.linkedIncomes > 0;

        return (
          <div className="flex items-center gap-2">
            {account.description}
            {hasLinkedData && (
              <div className="flex gap-2 ml-2">
                {account.linkedExpenses > 0 && (
                  <StatusBadge
                    color="yellow"
                    tooltip={`View ${account.linkedExpenses} linked ${account.linkedExpenses === 1 ? 'expense' : 'expenses'}`}
                    onClick={() => {
                      const accountId = account.rowId.toString();

                      // Persist account context before navigation so feature links and list pages
                      // resolve against the same account selection after route changes.
                      persistLinkedAccountFilter({
                        userId,
                        feature: 'expenses',
                        accountId,
                      });

                      navigate(`/expenses?accountId=${accountId}`);
                    }}
                    className="cursor-pointer hover:opacity-80 transition-opacity"
                  >
                    <BanknoteArrowDown />
                    {account.linkedExpenses}
                  </StatusBadge>
                )}
                {account.linkedIncomes > 0 && (
                  <StatusBadge
                    color="green"
                    tooltip={`View ${account.linkedIncomes} linked ${account.linkedIncomes === 1 ? 'income' : 'incomes'}`}
                    onClick={() => {
                      const accountId = account.rowId.toString();

                      // Persist account context before navigation so feature links and list pages
                      // resolve against the same account selection after route changes.
                      persistLinkedAccountFilter({
                        userId,
                        feature: 'incomes',
                        accountId,
                      });

                      navigate(`/incomes?accountId=${accountId}`);
                    }}
                    className="cursor-pointer hover:opacity-80 transition-opacity"
                  >
                    <BanknoteArrowUp />
                    {account.linkedIncomes}
                  </StatusBadge>
                )}
              </div>
            )}
          </div>
        );
      },
    },
    createMoneyValueColumn<Account>({
      accessorKey: 'balance',
      header: 'Balance',
      hint: 'The current account balance.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'reserved',
      header: 'Reserved',
      hint: 'Funds you have set aside and do not want to spend.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'available',
      header: 'Available',
      hint: 'What is left to spend: the balance less the reserved amount and your committed obligations.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'stableExpenseAccrual',
      header: 'Daily Need',
      hint: 'The stable daily amount to set aside to keep up with your recurring expenses.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'totalExpenseAccrued',
      header: 'Accrued',
      hint: 'The total amount set aside so far for the expense cycles currently in progress.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'totalArrears',
      header: 'Arrears',
      hint: 'The total amounts for overdue expenses.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createMoneyValueColumn<Account>({
      accessorKey: 'totalCommitted',
      header: 'Committed',
      hint: 'The total accrued amount plus arrears: what the balance is committed to covering.',
      options: {
        enableSorting: true,
        sortFn: 'basic',
      },
    }),
    createActionsColumn<Account>(account => (
      <AccountActions account={account} />
    )),
  ];

  // Row selection is off as there are no bulk actions.
  return (
    <>
      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
          <DataTable
            columns={columns}
            data={accounts}
            getRowId={createRowIdGetter<Account>()}
            highlightRowFilter={row =>
              row.original.rowId.toString() === editingId
            }
          />
        </CardContent>
      </Card>
    </>
  );
}

export default AccountsTable;
export type { AccountsTableProps };

