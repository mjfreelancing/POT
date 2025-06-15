import { FailResultBase, Result, SuccessResult } from '@/lib';

import { useGet } from './useApi';
import { Projection } from '@/data/projection';

const useApiGetProjection = () => {
  const query = useGet<Projection>('/projections', ['projections']);
  const result = query.data as Result<Projection, FailResultBase>;

  let data: Result<Projection, FailResultBase>;

  if (result?.success) {
    data = new SuccessResult(result.value);
  } else {
    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    data = result;
  }

  // Returns isLoading, isError, error, and data (and anything else from React Query)
  return { ...query, data };
};

export { useApiGetProjection };
