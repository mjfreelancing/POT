import { useGet, usePost, usePut } from '../../hooks/useApi';
import { Account } from '@/data/accounts/account';
import { compareAccountBsbNumber } from '@/data/accounts/account-comparators';

const useGetAllAccounts = () => {
  const { data, ...rest } = useGet<Account[]>('/accounts', ['accounts']);
  const sorted = data?.sort(compareAccountBsbNumber);
  return { data: sorted, ...rest };
};

const useGetAccountById = (id: string) => {
  return useGet<Account>(`/accounts/${id}`, ['accounts', id]);
};

const useCreateAccount = () => {
  return usePost<void, Account>('/accounts');
};

const useUpdateAccount = () => {
  return usePut<void, Account>('/accounts');
};

export {
  useGetAllAccounts,
  useGetAccountById,
  useCreateAccount,
  useUpdateAccount,
};
