import type { Identity } from '@/data';
import type { DeleteSite, EditSite } from '@/data/site';
import type { FailResultBase, Result } from '@/lib';

import { useDelete, usePutWithId } from './useApi';

const useApiUpdateSite = () => {
  const mutation = usePutWithId<Identity, EditSite>(
    (id: string) => `/sites/${id}`,
  );
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

const useApiDeleteSite = () => {
  const mutation = useDelete<void, DeleteSite>('/sites/current');

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

export { useApiDeleteSite, useApiUpdateSite };
