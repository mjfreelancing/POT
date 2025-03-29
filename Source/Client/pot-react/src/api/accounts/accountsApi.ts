import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { Account } from '@/data/accounts/account';

import { ApiBase } from '../apiBase';

class AccountsApi extends ApiBase {
  public getAll(signal: AbortSignal): Promise<Account[]> {
    console.info('AccountsApi.getAll');

    return this.get<Account[]>('/accounts', signal);
  }

  public getById(id: string, signal: AbortSignal): Promise<Account> {
    console.info('AccountsApi.getById');

    return this.get<Account>(`/accounts/${id}`, signal);
  }

  public create(data: Account, signal: AbortSignal): Promise<void> {
    console.info('AccountsApi.create');

    return this.post<void, Account>('/accounts', data, signal);
  }

  public update(data: Account, signal: AbortSignal): Promise<void> {
    console.info('AccountsApi.update');

    return this.put<void, Account>('/accounts', data, signal);
  }
}

const accountsApi = new AccountsApi();

const getAccounts = async (signal: AbortSignal): Promise<Account[]> => {
  return accountsApi.getAll(signal);
};

const getAccountById = async (
  id: string,
  signal: AbortSignal,
): Promise<Account> => {
  return accountsApi.getById(id, signal);
};

const updateAccount = async (
  account: Account,
  signal: AbortSignal,
): Promise<void> => {
  return accountsApi.update(account, signal);
};

const useAllAccountsQuery = () => {
  return useQuery({
    queryKey: ['accounts'],
    queryFn: async ({ signal }) => getAccounts(signal),
    retryDelay: 10000,
  });
};

const useAccountByIdQuery = (id: string) => {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: async ({ signal }) => getAccountById(id, signal),
    enabled: !!id,
  });
};

const useUpdateAccountMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      account,
      signal,
    }: {
      account: Account;
      signal: AbortSignal;
    }) => updateAccount(account, signal),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};

export {
  getAccountById,
  getAccounts,
  updateAccount,
  useAccountByIdQuery,
  useAllAccountsQuery,
  useUpdateAccountMutation,
};
