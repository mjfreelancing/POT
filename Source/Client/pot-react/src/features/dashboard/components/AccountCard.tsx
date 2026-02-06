import StatusBadge from '@/components/feedback/badge/StatusBadge';
import { Card, CardContent } from '@/components/ui/card';
import type { Account } from '@/data';
import { formatMoneyValue } from '@/lib';
import { cn } from '@/lib/utils';

type AccountCardProps = {
  /** The account data to display */
  account: Account;
};

/**
 * A card component to display individual account information.
 * Shows account name, BSB, account number, balance, available funds, and status.
 *
 * @example
 * <AccountCard account={accountData} />
 */
function AccountCard({ account }: AccountCardProps) {
  const { description, bsb, number, balance, available } = account;

  // Determine account status based on available funds
  const getAccountStatus = () => {
    if (available < 0) return 'overdrawn';
    if (available < balance * 0.1) return 'low';
    return 'healthy';
  };

  const status = getAccountStatus();

  // Status badge configuration
  const statusConfig = {
    overdrawn: {
      label: 'Overdrawn',
      color: 'red' as const,
      borderClass:
        'border-red-500/30 bg-red-50/50 dark:bg-red-950/20 border-l-2 border-l-red-500',
    },
    low: {
      label: 'Low',
      color: 'orange' as const,
      borderClass: 'border-orange-500/30 bg-orange-50/50 dark:bg-orange-950/20',
    },
    healthy: {
      label: 'Healthy',
      color: 'green' as const,
      borderClass: 'border-green-500/30 bg-green-50/50 dark:bg-green-950/20',
    },
  };

  const config = statusConfig[status];

  return (
    <Card
      className={cn(
        'transition-all duration-200 hover:shadow-md',
        config.borderClass,
      )}
    >
      <CardContent className="p-3 lg:p-4">
        <div className="space-y-2 lg:space-y-3">
          {/* Account Name */}
          <div>
            <h3 className="font-bold text-base lg:text-lg leading-tight text-green-700 dark:text-green-300">
              {description}
            </h3>
            <div className="text-[10px] lg:text-xs text-muted-foreground mt-1 space-y-0.5">
              <div>BSB: {bsb}</div>
              <div>Acc: {number}</div>
            </div>
          </div>

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

          {/* Status Badge */}
          <div className="flex justify-end">
            <StatusBadge
              color={config.color}
              className="w-16 lg:w-24 text-[10px] lg:text-sm"
            >
              {config.label}
            </StatusBadge>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default AccountCard;
export type { AccountCardProps };
