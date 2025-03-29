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
  const [originalValues, setOriginalValues] = useState<Account | null>(null);

  useEffect(() => {
    if (!originalValues) {
      setOriginalValues(account);

      form.reset({
        bsb: account.bsb,
        number: account.number,
        description: account.description,
        balance: account.balance,
        reserved: account.reserved,
      });
    }
  }, [account, form, originalValues]);

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

  return { resetToOriginal };
};
