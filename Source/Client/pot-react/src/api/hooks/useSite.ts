import type { Identity } from '@/data';
import type { EditSite } from '@/data/site';
import type { FailResultBase, Result } from '@/lib';

import { usePutWithId } from './useApi';

const useApiUpdateSite = () => {
  const mutation = usePutWithId<Identity, EditSite>(
    (id: string) => `/sites/${id}`,
  );
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

export { useApiUpdateSite };
