import { ErrorSheet } from '@/components/feedback';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { Account } from '@/data';

import AccountMobileCard from './AccountMobileCard';

type AccountCardGridProps = {
  accounts: Account[];
};

/**
 * Grid view for displaying accounts as cards on mobile devices.
 * Uses 2-column grid layout for optimal mobile viewing.
 */
function AccountCardGrid({ accounts }: AccountCardGridProps) {
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
          <div className="grid grid-cols-2 gap-3">
            {accounts.map(account => (
              <AccountMobileCard key={account.rowId} account={account} />
            ))}
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default AccountCardGrid;
export type { AccountCardGridProps };
