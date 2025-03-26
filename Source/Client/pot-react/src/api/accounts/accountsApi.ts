import { Account } from '@/data/accounts/account';

import { ApiBase } from '../apiBase';
import { logCall } from '@/lib/loggerUtils';

class AccountsApi extends ApiBase {
  public getAll(signal: AbortSignal): Promise<Account[]> {
    logCall(this, this.getAll);

    return this.get<Account[]>('/accounts', signal);
  }

  public getById(id: string, signal: AbortSignal): Promise<Account> {
    logCall(this, this.getById);

    return this.get<Account>(`/accounts/${id}`, signal);
  }

  public create(data: Account, signal: AbortSignal): Promise<void> {
    logCall(this, this.create);

    return this.post<void, Account>('/accounts', data, signal);
  }
}

const accountsApi = new AccountsApi();

export const getAccounts = async (signal: AbortSignal): Promise<Account[]> => {
  return accountsApi.getAll(signal);
};
