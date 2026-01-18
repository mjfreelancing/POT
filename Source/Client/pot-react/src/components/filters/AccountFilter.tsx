import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { Account } from '@/data';

type AccountFilterProps = {
  accounts: Account[];
  selectedAccountId: string | null;
  onAccountChange: (accountId: string | null) => void;
};

function AccountFilter({
  accounts,
  selectedAccountId,
  onAccountChange,
}: AccountFilterProps) {
  const isActive = selectedAccountId !== null;

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor="account-filter-trigger" className="sr-only">
        Filter by account
      </label>
      <Select
        value={selectedAccountId ?? 'all'}
        onValueChange={value => {
          onAccountChange(value === 'all' ? null : value);
        }}
        name="account-filter"
      >
        <SelectTrigger
          className={`w-[280px] ${isActive ? 'ring-[2px] ring-primary/60 bg-primary/10' : ''}`}
          id="account-filter-trigger"
          aria-label="Filter by account"
        >
          <SelectValue placeholder="Filter by account" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Accounts</SelectItem>
          {accounts.map(account => (
            <SelectItem key={account.rowId} value={account.rowId.toString()}>
              {account.description}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

export default AccountFilter;
export type { AccountFilterProps };
