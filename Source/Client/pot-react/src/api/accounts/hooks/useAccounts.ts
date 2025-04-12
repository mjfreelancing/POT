import {
  Account,
  compareAccountBsbNumber,
  CreateAccount,
  EditAccount,
} from '@/data/accounts/account';

import { useGet, usePost, usePut } from '../../hooks/useApi';

const useGetAllAccounts = () => {
  const { data, ...rest } = useGet<Account[]>('/accounts', ['accounts']);
  const sorted = data?.sort(compareAccountBsbNumber);
  return { data: sorted, ...rest };
};

const useGetAccountById = (id: string) => {
  return useGet<Account>(`/accounts/${id}`, ['accounts', id]);
};

const useCreateAccount = () => {
  return usePost<void, CreateAccount>('/accounts');
};

const useUpdateAccount = () => {
  return usePut<void, EditAccount>('/accounts');
};

export {
  useCreateAccount,
  useGetAccountById,
  useGetAllAccounts,
  useUpdateAccount,
};
