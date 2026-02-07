import {
  BanknoteArrowDown,
  BanknoteArrowUp,
  MoreHorizontal,
  Pencil,
  Trash2,
} from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router';

import { ConfirmationDialog } from '@/components/dialog';
import { ErrorSheet, StatusBadge } from '@/components/feedback';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';
import { WithPermission } from '@/features/auth/components';
import { formatMoneyValue } from '@/lib';
import { cn } from '@/lib/utils';

import useDeleteAccount from '../delete/hooks/useDeleteAccount';

type AccountMobileCardProps = {
  account: Account;
};

/**
 * Mobile card component for displaying account information.
 * Styled consistently with dashboard AccountCard but with action menu instead of status badge.
 */
function AccountMobileCard({ account }: AccountMobileCardProps) {
  const { description, bsb, number, balance, available } = account;
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { error, setError } = useErrorContext();
  const navigate = useNavigate();
  const { deleteAccount } = useDeleteAccount(account.rowId);

  // Determine account status for styling (same logic as dashboard)
  const getAccountStatus = () => {
    if (available < 0) return 'overdrawn';
    if (available < balance * 0.1) return 'low';
    return 'healthy';
  };

  const status = getAccountStatus();

  // Status styling configuration (same as dashboard)
  const statusConfig = {
    overdrawn: {
      borderClass:
        'border-red-500/30 bg-red-50/50 dark:bg-red-950/20 border-l-2 border-l-red-500',
    },
    low: {
      borderClass: 'border-orange-500/30 bg-orange-50/50 dark:bg-orange-950/20',
    },
    healthy: {
      borderClass: 'border-green-500/30 bg-green-50/50 dark:bg-green-950/20',
    },
  };

  const config = statusConfig[status];

  const handleDelete = async () => {
    const result = await deleteAccount();
    setShowDeleteDialog(false);

    if (!result.success) {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <Card
        className={cn(
          'transition-all duration-200 hover:shadow-md py-2 lg:py-2.5 gap-0 flex flex-col',
          config.borderClass,
        )}
      >
        <CardContent className="px-2 lg:px-2.5 flex flex-col flex-1">
          <div className="flex flex-col h-full">
            {/* Account Name */}
            <div className="mb-0.5">
              <h3 className="font-bold text-base lg:text-lg leading-tight text-green-700 dark:text-green-300">
                {description}
              </h3>
              <div className="text-[10px] lg:text-xs text-muted-foreground mt-1 space-y-0.5">
                <div>BSB: {bsb}</div>
                <div>Acc: {number}</div>
              </div>
            </div>

            {/* Spacer to push content to bottom */}
            <div className="flex-1" />

            {/* Bottom section with divider, balance, and actions */}
            <div className="space-y-1.5 lg:space-y-2">
              {/* Divider */}
              <div className="border-t border-border" />

              {/* Balance Information */}
              <div className="space-y-1 lg:space-y-1.5">
                <div className="flex justify-between items-center">
                  <span className="text-[11px] lg:text-sm font-medium text-foreground">
                    Balance:
                  </span>
                  <span
                    className={cn(
                      'text-sm lg:text-lg font-semibold',
                      balance >= 0
                        ? 'text-success'
                        : 'text-destructive-high-contrast',
                    )}
                  >
                    {formatMoneyValue(balance)}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[11px] lg:text-sm font-medium text-foreground">
                    Available:
                  </span>
                  <span
                    className={cn(
                      'text-sm lg:text-lg font-semibold',
                      available >= 0
                        ? 'text-success'
                        : 'text-destructive-high-contrast',
                    )}
                  >
                    {formatMoneyValue(available)}
                  </span>
                </div>
              </div>

              {/* Linked Data Badges (bottom left) and Action Menu (bottom right) */}
              <div className="flex justify-between items-center mt-4">
                {/* Linked Data Badges */}
                {(account.linkedExpenses > 0 || account.linkedIncomes > 0) && (
                  <div className="flex gap-1.5">
                    {account.linkedExpenses > 0 && (
                      <StatusBadge
                        color="yellow"
                        tooltip={`View ${account.linkedExpenses} linked ${account.linkedExpenses === 1 ? 'expense' : 'expenses'}`}
                        onClick={() =>
                          navigate(`/expenses?accountId=${account.rowId}`)
                        }
                        className="cursor-pointer hover:opacity-80 transition-opacity h-8 flex items-center"
                      >
                        <BanknoteArrowDown className="h-3 w-3" />
                        {account.linkedExpenses}
                      </StatusBadge>
                    )}
                    {account.linkedIncomes > 0 && (
                      <StatusBadge
                        color="green"
                        tooltip={`View ${account.linkedIncomes} linked ${account.linkedIncomes === 1 ? 'income' : 'incomes'}`}
                        onClick={() =>
                          navigate(`/incomes?accountId=${account.rowId}`)
                        }
                        className="cursor-pointer hover:opacity-80 transition-opacity h-8 flex items-center"
                      >
                        <BanknoteArrowUp className="h-3 w-3" />
                        {account.linkedIncomes}
                      </StatusBadge>
                    )}
                  </div>
                )}

                {/* Action Menu */}
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
                    <WithPermission permissions={['account:manage']} mode="all">
                      <DropdownMenuItem
                        onClick={() =>
                          navigate(`/accounts/edit/${account.rowId}`)
                        }
                      >
                        <Pencil className="mr-2 h-4 w-4" />
                        Edit
                      </DropdownMenuItem>
                    </WithPermission>

                    <WithPermission permissions={['account:manage']} mode="all">
                      <DropdownMenuItem
                        className="text-destructive-high-contrast"
                        disabled={
                          account.linkedExpenses > 0 ||
                          account.linkedIncomes > 0
                        }
                        onClick={() => setShowDeleteDialog(true)}
                      >
                        <Trash2 className="mr-2 h-4 w-4 text-destructive-high-contrast" />
                        Delete
                      </DropdownMenuItem>
                    </WithPermission>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <ConfirmationDialog
        open={showDeleteDialog}
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteDialog(false)}
        title="Delete Account"
        description={`Are you sure you want to delete "${account.description}"? This action cannot be undone.`}
        confirmLabel="Delete"
      />
    </>
  );
}

export default AccountMobileCard;
export type { AccountMobileCardProps };
