import { useEffect, useState } from 'react';
import { UseFormReturn } from 'react-hook-form';

import { Account } from '@/data/accounts/account';

type AccountFormValues = {
  bsb: string;
  number: string;
  description: string;
  balance: number;
  reserved: number;
};

export const useAccountEditor = (
  form: UseFormReturn<AccountFormValues>,
  account: Account,
) => {
  const [originalValues, setOriginalValues] =
    useState<AccountFormValues | null>(null);

  useEffect(() => {
    setOriginalValues({
      bsb: account.bsb,
      number: account.number,
      description: account.description,
      balance: account.balance,
      reserved: account.reserved,
    });

    form.reset({
      bsb: account.bsb,
      number: account.number,
      description: account.description,
      balance: account.balance,
      reserved: account.reserved,
    });
  }, [account, form]);

  // Subscribe to form changes for the current values so we can custom compare.
  // This is required because the user edits are returned as strings from form.getValues().
  form.watch(['balance', 'reserved']);

  const resetToOriginal = () => {
    if (originalValues) {
      form.reset({
        bsb: originalValues.bsb,
        number: originalValues.number,
        description: originalValues.description,
        balance: originalValues.balance,
        reserved: originalValues.reserved,
      });
    }
  };

  const isDirty = () => {
    if (!originalValues) {
      return false;
    }

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
};
