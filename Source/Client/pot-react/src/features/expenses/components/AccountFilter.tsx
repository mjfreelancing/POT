import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Account } from '@/data';

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
  return (
    <Select
      value={selectedAccountId ?? 'all'}
      onValueChange={value => {
        onAccountChange(value === 'all' ? null : value);
      }}
    >
      <SelectTrigger className="w-[200px]">
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
  );
}

export default AccountFilter;
