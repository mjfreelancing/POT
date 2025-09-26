import { Projection } from '@/data/projection';
import { FailResultBase, Result, SuccessResult } from '@/lib';

import { useGet } from './useApi';

const useApiGetProjection = (startDate: string, endDate: string) => {
  const query = useGet<Projection>(
    `/projections?startDate=${startDate}&endDate=${endDate}`,
    ['projections', startDate, endDate],
  );

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
