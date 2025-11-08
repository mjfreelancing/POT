import type { UseQueryOptions } from '@tanstack/react-query';
import { useMutation, useQuery } from '@tanstack/react-query';
import type { AxiosResponse } from 'axios';
import axios from 'axios';

import type { FailResult, FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

type MutationData<TData> = {
  data: TData;
  signal?: AbortSignal;
};

type MutationDataWithId<TData> = {
  id: string;
  data: TData;
  signal?: AbortSignal;
};

type DeleteMutationData = {
  signal?: AbortSignal;
};

type MutationOperation<TResponse> = () => Promise<AxiosResponse<TResponse>>;

// Options type for useGet that excludes queryKey and queryFn since they are
// handled internally by the hook. This ensures type safety while allowing
// consumers to customize other React Query options like caching and refetching.
type GetOptions<TResponse> = Omit<
  UseQueryOptions<
    Result<TResponse, FailResultBase>,
    unknown,
    Result<TResponse, FailResultBase>,
    string[]
  >,
  'queryKey' | 'queryFn'
>;

const performOperation = async <TResponse>(
  operation: MutationOperation<TResponse>,
): Promise<Result<TResponse, FailResultBase>> => {
  try {
    const response = await operation();
    return new SuccessResult(response.data);
  } catch (error) {
    return error as FailResult<FailResultBase>;
  }
};

/**
 * useDelete is a utility hook for making DELETE requests.
 *
 * @template TResponse - The expected response type from the API.
 *
 * @param {string} url - The endpoint URL for the DELETE request.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, { id: string }, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * ```typescript
 * const mutation = useDelete<MyResponseType>('/api/resource');
 * mutation.mutate({ id: '123' });
 * ```
 */
const useDelete = <TResponse>(url: string) => {
  return useMutation({
    mutationFn: async ({
      signal,
    }: DeleteMutationData): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() => axios.delete<TResponse>(url, { signal }));
    },
  });
};

/**
 * useGet is a utility hook for making GET requests.
 *
 * @template TResponse - The expected response type from the API.
 *
 * @param {string} url - The endpoint URL for the GET request.
 * @param {string[]} queryKey - The unique key for caching and identifying the query.
 * @param {GetOptions<TResponse>} [options] - Optional configuration for the query.
 *
 * @returns {UseQueryResult<Result<TResponse, FailResultBase>, unknown>} -
 * A query object from react-query, with the query function and state.
 *
 * ### Usage:
 * ```typescript
 * const { data, isLoading } = useGet<MyResponseType>('/api/resource', ['resource']);
 * ```
 */
const useGet = <TResponse>(
  url: string,
  queryKey: string[],
  options: GetOptions<TResponse> = {},
) => {
  return useQuery({
    ...options,
    queryKey,
    queryFn: async ({ signal }): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() => axios.get<TResponse>(url, { signal }));
    },
    placeholderData: prev => prev,
  });
};

/**
 * usePost is a utility hook for making POST requests.
 *
 * @template TResponse - The expected response type from the API.
 * @template TData - The type of data to be sent in the request body. Defaults to void for no data.
 *
 * @param {string} url - The endpoint URL for the POST request.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, Partial<MutationData<TData>>, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * #### With Data:
 * ```typescript
 * const mutation = usePost<MyResponseType, MyRequestType>('/api/resource');
 * mutation.mutate({ data: { key: 'value' } });
 * ```
 *
 * #### Without Data:
 * ```typescript
 * const mutation = usePost<MyResponseType>('/api/resource');
 * mutation.mutate();
 * ```
 */
const usePost = <TResponse, TData = void>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: Partial<MutationData<TData>> = {}): Promise<
      Result<TResponse, FailResultBase>
    > => {
      return performOperation(() =>
        axios.post<TResponse>(url, data, { signal }),
      );
    },
  });
};

/**
 * usePut is a utility hook for making PUT requests.
 *
 * @template TResponse - The expected response type from the API.
 * @template TData - The type of data to be sent in the request body.
 *
 * @param {string} url - The endpoint URL for the PUT request.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, MutationData<TData>, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * ```typescript
 * const mutation = usePut<MyResponseType, MyRequestType>('/api/resource');
 * mutation.mutate({ data: { key: 'value' } });
 * ```
 */

const usePut = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: MutationData<TData>): Promise<Result<TResponse, FailResultBase>> => {
      return performOperation(() =>
        axios.put<TResponse>(url, data, { signal }),
      );
    },
  });
};

/**
 * usePutWithId is a utility hook for making PUT requests to endpoints requiring an ID.
 *
 * @template TResponse - The expected response type from the API.
 * @template TData - The type of data to be sent in the request body.
 *
 * @param {(id: string) => string} urlFn - A function that generates the endpoint URL using an ID.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, { id: string; data: TData }, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * #### With Data:
 * ```typescript
 * const mutation = usePutWithId<MyResponseType, MyRequestType>((id) => `/api/resource/${id}`);
 * mutation.mutate({ id: '123', data: { key: 'value' } });
 * ```
 *
 * #### Without Data (void):
 * ```typescript
 * const mutation = usePutWithId<MyResponseType, void>((id) => `/api/resource/${id}`);
 * mutation.mutate({ id: '123' });
 * ```
 */

const usePutWithId = <TResponse, TData>(urlFn: (id: string) => string) => {
  return useMutation({
    mutationFn: async ({
      id,
      data,
      signal,
    }: MutationDataWithId<TData>): Promise<
      Result<TResponse, FailResultBase>
    > => {
      const url = urlFn(id);
      return performOperation(() =>
        axios.put<TResponse>(url, data, { signal }),
      );
    },
  });
};

/**
 * usePutWithIdNoData is a utility hook for making PUT requests to endpoints requiring an ID but no request body.
 *
 * @template TResponse - The expected response type from the API.
 *
 * @param {(id: string) => string} urlFn - A function that generates the endpoint URL using an ID.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, { id: string; signal?: AbortSignal }, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * ```typescript
 * const mutation = usePutWithIdNoData<MyResponseType>((id) => `/api/resource/${id}/action`);
 * mutation.mutate({ id: '123' });
 * ```
 */
const usePutWithIdNoData = <TResponse>(urlFn: (id: string) => string) => {
  return useMutation({
    mutationFn: async ({
      id,
      signal,
    }: {
      id: string;
      signal?: AbortSignal;
    }): Promise<Result<TResponse, FailResultBase>> => {
      const url = urlFn(id);
      return performOperation(() =>
        axios.put<TResponse>(url, undefined, { signal }),
      );
    },
  });
};

/**
 * usePostWithId is a utility hook for making POST requests to endpoints requiring an ID.
 *
 * @template TResponse - The expected response type from the API.
 * @template TData - The type of data to be sent in the request body.
 *
 * @param {(id: string) => string} urlFn - A function that generates the endpoint URL using an ID.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, { id: string; data: TData }, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * #### With Data:
 * ```typescript
 * const mutation = usePostWithId<MyResponseType, MyRequestType>((id) => `/api/resource/${id}`);
 * mutation.mutate({ id: '123', data: { key: 'value' } });
 * ```
 *
 * #### Without Data (void):
 * ```typescript
 * const mutation = usePostWithId<MyResponseType, void>((id) => `/api/resource/${id}`);
 * mutation.mutate({ id: '123' });
 * ```
 */

const usePostWithId = <TResponse, TData>(urlFn: (id: string) => string) => {
  return useMutation({
    mutationFn: async ({
      id,
      data,
      signal,
    }: MutationDataWithId<TData>): Promise<
      Result<TResponse, FailResultBase>
    > => {
      const url = urlFn(id);
      return performOperation(() =>
        axios.post<TResponse>(url, data, { signal }),
      );
    },
  });
};

/**
 * usePostWithIdNoData is a utility hook for making POST requests to endpoints requiring an ID but no request body.
 *
 * @template TResponse - The expected response type from the API.
 *
 * @param {(id: string) => string} urlFn - A function that generates the endpoint URL using an ID.
 *
 * @returns {UseMutationResult<Result<TResponse, FailResultBase>, unknown, { id: string; signal?: AbortSignal }, unknown>} -
 * A mutation object from react-query, with the mutation function and state.
 *
 * ### Usage:
 * ```typescript
 * const mutation = usePostWithIdNoData<MyResponseType>((id) => `/api/resource/${id}/action`);
 * mutation.mutate({ id: '123' });
 * ```
 */
const usePostWithIdNoData = <TResponse>(urlFn: (id: string) => string) => {
  return useMutation({
    mutationFn: async ({
      id,
      signal,
    }: {
      id: string;
      signal?: AbortSignal;
    }): Promise<Result<TResponse, FailResultBase>> => {
      const url = urlFn(id);
      return performOperation(() =>
        axios.post<TResponse>(url, undefined, { signal }),
      );
    },
  });
};

export type { DeleteMutationData, MutationData, MutationDataWithId };
export {
  useDelete,
  useGet,
  usePost,
  usePostWithId,
  usePostWithIdNoData,
  usePut,
  usePutWithId,
  usePutWithIdNoData,
};
