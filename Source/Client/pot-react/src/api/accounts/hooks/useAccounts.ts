import { useGet, usePost, usePut } from '../../hooks/useApi';
import { Account } from '@/data/accounts/account';

const useGetAllAccounts = () => {
  return useGet<Account[]>('/accounts', ['accounts']);
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
