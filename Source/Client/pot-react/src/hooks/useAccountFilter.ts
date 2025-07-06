import { useEffect, useMemo, useState } from 'react';

import { Account } from '@/data';

/**
 * Represents any item that can be associated with an account.
 * The account property is optional and can be null/undefined for unassigned items.
 */
type ItemWithAccount = {
  account?: {
    rowId: string | number;
  } | null;
};

/**
 * Configuration options for the useAccountFilter hook.
 */
type UseAccountFilterOptions = {
  /** All available accounts in the system */
  accounts: Account[];
  /** Array of items (expenses, incomes, etc.) that can be filtered by account */
  items: ItemWithAccount[];
};

/**
 * Return type for the useAccountFilter hook.
 */
type UseAccountFilterReturn<T extends ItemWithAccount> = {
  /**
   * Filtered list of accounts that actually have items associated with them.
   * Includes a virtual "Not Assigned" account if there are unassigned items.
   * Sorted alphabetically by description.
   */
  accountsInItems: Account[];

  /**
   * Currently selected account ID for filtering, or null for no filter.
   * Can be 'not-assigned' for the virtual "Not Assigned" account.
   */
  selectedAccountId: string | null;

  /**
   * Function to update the selected account filter.
   * Pass null to clear the filter and show all items.
   */
  setSelectedAccountId: (accountId: string | null) => void;

  /**
   * Items filtered by the selected account.
   * If no account is selected, returns all items.
   * If 'not-assigned' is selected, returns only items without an account.
   */
  filteredItems: T[];
};

/**
 * Custom hook for filtering items by account with intelligent account selection.
 *
 * This hook provides account-based filtering functionality for any items that can be
 * associated with accounts (expenses, incomes, etc.). It automatically:
 * - Shows only accounts that actually have items associated with them
 * - Includes a virtual "Not Assigned" option for unassigned items
 * - Maintains filter state and handles account selection changes
 * - Automatically resets selection if the selected account becomes unavailable
 *
 * @template T - The type of items being filtered (must extend ItemWithAccount)
 *
 * @param options - Configuration object
 * @param options.accounts - All available accounts in the system
 * @param options.items - Array of items to be filtered by account
 *
 * @returns Object containing:
 * - accountsInItems: Filtered accounts that have associated items + "Not Assigned" if needed
 * - selectedAccountId: Currently selected account ID or null for no filter
 * - setSelectedAccountId: Function to change the account filter
 * - filteredItems: Items filtered by the selected account
 *
 * @example
 * ```typescript
 * const {
 *   accountsInItems,
 *   selectedAccountId,
 *   setSelectedAccountId,
 *   filteredItems
 * } = useAccountFilter({
 *   accounts: allAccounts,
 *   items: expenses
 * });
 *
 * // Use in JSX
 * <AccountFilter
 *   accounts={accountsInItems}
 *   selectedAccountId={selectedAccountId}
 *   onAccountChange={setSelectedAccountId}
 * />
 * <DataTable data={filteredItems} />
 * ```
 */
function useAccountFilter<T extends ItemWithAccount>({
  accounts,
  items,
}: UseAccountFilterOptions & { items: T[] }): UseAccountFilterReturn<T> {
  // State to track which account is currently selected for filtering
  const [selectedAccountId, setSelectedAccountId] = useState<string | null>(
    null,
  );

  // This block handles both account addition and removal by dynamically
  // building the filter list based on which accounts actually have items.
  // When data changes, the filter automatically updates to show only
  // relevant accounts, including/excluding the "Not Assigned" option.
  const accountsInItems = useMemo(() => {
    const uniqueAccountIds = new Set<string>();
    const accountsMap = new Map<string, Account>();
    let hasUnassignedItems = false;

    // Create a map of all accounts for quick lookup by ID
    accounts.forEach(account => {
      accountsMap.set(account.rowId.toString(), account);
    }); // Scan through all items to find which accounts are actually used

    items.forEach((item: T) => {
      if (item.account?.rowId) {
        // Item has an account assigned - add to our set of used accounts
        uniqueAccountIds.add(item.account.rowId.toString());
      } else {
        // Item has no account assigned - remember this for "Not Assigned" option
        hasUnassignedItems = true;
      }
    });

    // Build the final list: only accounts that are actually used by items
    const accountsInUse = Array.from(uniqueAccountIds)
      .map(accountId => accountsMap.get(accountId))
      .filter((account): account is Account => account !== undefined)
      .sort((lhs, rhs) => lhs.description.localeCompare(rhs.description));

    // Add a virtual "Not Assigned" account if there are unassigned items.
    // This allows users to filter specifically for items without accounts.
    if (hasUnassignedItems) {
      accountsInUse.unshift({
        rowId: 'not-assigned',
        description: 'Not Assigned',
        bsb: '',
        number: '',
        balance: 0,
        reserved: 0,
        totalExpenseAccrued: 0,
        dailyExpenseAccrual: 0,
        available: 0,
        linkedExpenses: 0,
        linkedIncomes: 0,
        etag: 0n,
      });
    }
    return accountsInUse;
  }, [items, accounts]);

  // This block handles the case where a user has selected an account
  // for filtering, but that account is no longer available (e.g., all
  // items for that account were deleted). It automatically clears the
  // selection to prevent invalid filter states.
  useEffect(() => {
    if (selectedAccountId && accountsInItems.length > 0) {
      const isSelectedAccountPresent = accountsInItems.some(
        account => account.rowId.toString() === selectedAccountId,
      );

      if (!isSelectedAccountPresent) {
        // Selected account is no longer available, clear the filter
        setSelectedAccountId(null);
      }
    }
  }, [selectedAccountId, accountsInItems]);

  // This block applies the actual filtering logic to show only items
  // that match the selected account. It automatically updates when
  // items are added/removed or when the selection changes, ensuring
  // the filtered results stay current with the data.
  const filteredItems = useMemo(() => {
    if (!selectedAccountId) {
      // No filter selected - return all items
      return items;
    }

    if (selectedAccountId === 'not-assigned') {
      // Special case: filter for items with no account assigned
      return items.filter((item: T) => !item.account?.rowId);
    }

    // Normal case: filter for items with the specific account ID
    return items.filter(
      (item: T) => item.account?.rowId.toString() === selectedAccountId,
    );
  }, [items, selectedAccountId]);

  return {
    accountsInItems,
    selectedAccountId,
    setSelectedAccountId,
    filteredItems,
  };
}

export default useAccountFilter;
