import { ApiBase } from '../apiBase';
import { Account } from './account';

class AccountsApi extends ApiBase {
  public getAll(signal: AbortSignal): Promise<Account[]> {
    return this.get<Account[]>('/accounts', signal);
  }

  public getById(id: string, signal: AbortSignal): Promise<Account> {
    return this.get<Account>(`/accounts/${id}`, signal);
  }

  public create(data: Account, signal: AbortSignal): Promise<void> {
    return this.post<void, Account>('/accounts', data, signal);
  }
}

const accountsApi = new AccountsApi();

export const getAccounts = async (signal: AbortSignal): Promise<Account[]> => {
  return accountsApi.getAll(signal);
};
