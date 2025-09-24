import { useQueryClient } from '@tanstack/react-query';

import { useApiUpdateAccount } from '@/api/hooks';
import { EditAccount, Identity } from '@/data';
import { FailResultBase, Result } from '@/lib';
import { logger } from '@/lib/logging';

function useEditAccount() {
  const queryClient = useQueryClient();
  const apiUpdateAccount = useApiUpdateAccount();

  async function editAccount(
    id: string,
    account: EditAccount,
  ): Promise<Result<Identity, FailResultBase>> {
    const controller = new AbortController();

    logger.info('Accounts', 'Edit account started', account);
    try {
      // Not using onSuccess callback because both success/fails are returned
      const result = await apiUpdateAccount.mutateAsync({
        id,
        data: account,
        signal: controller.signal,
      });

      if (result.success) {
        logger.info('Accounts', 'Edit account successful', result.value);
        queryClient.invalidateQueries({ queryKey: ['accounts'] });
      } else {
        logger.error('Accounts', 'Edit account failed', result.error);
      }

      return result;
    } catch (error) {
      logger.error('Accounts', 'Edit account error', error);
      throw error;
    } finally {
      controller.abort();
    }
  }

  return { editAccount };
}

export default useEditAccount;
