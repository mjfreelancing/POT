import { useEffect, useMemo } from 'react';
import type { UseFormReturn } from 'react-hook-form';

import type { Account } from '@/data';

type AccountFormValues = {
  bsb: string;
  number: string;
  description: string;
  balance: number;
  reserved: number;
};

function toAccountFormValues(account: Account): AccountFormValues {
  return {
    bsb: account.bsb,
    number: account.number,
    description: account.description,
    balance: account.balance,
    reserved: account.reserved,
  };
}

function useAccountEditor(
  form: UseFormReturn<AccountFormValues>,
  account: Account,
) {
  // Snapshot of the loaded values so edits can be detected and reverted. Derived
  // (not stored) so there is no state to reset when the account changes.
  const originalValues = useMemo(() => toAccountFormValues(account), [account]);

  // Keep the form in sync when the account under edit changes. Resetting the
  // form is an external (react-hook-form) update rather than React state, so it
  // runs in an effect without tripping the set-state-in-effect rule.
  useEffect(() => {
    form.reset(originalValues);
  }, [form, originalValues]);

  // Watch form changes to trigger re-renders when these fields change. This ensures isDirty()
  // uses current values since balance/reserved are returned as strings from form.getValues().
  form.watch(['balance', 'reserved']);

  const resetToOriginal = () => {
    form.reset(originalValues);
  };

  const isDirty = () => {
    const currentValues = form.getValues();

    const currentBalance = Number(currentValues.balance);
    const currentReserved = Number(currentValues.reserved);

    return (
      currentValues.bsb !== originalValues.bsb ||
      currentValues.number !== originalValues.number ||
      currentValues.description !== originalValues.description ||
      currentBalance !== originalValues.balance ||
      currentReserved !== originalValues.reserved
    );
  };

  return { resetToOriginal, isDirty };
}

export default useAccountEditor;
