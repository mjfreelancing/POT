import type { Identity } from '@/data';
import type { CreateUser, EditUser, User } from '@/data/user';
import { compareUserDisplayName } from '@/data/user';
import { type FailResultBase, type Result, SuccessResult } from '@/lib';

import { useDelete, useGet, usePost, usePutWithId } from './useApi';

const useApiGetAllUsers = () => {
  const query = useGet<User[]>('/users', ['users']);
  const result = query.data as Result<User[], FailResultBase>;

  let data: Result<User[], FailResultBase>;

  if (result?.success) {
    // spreading [...result.value] to create a shallow copy of the array since
    // sort() mutates the source array in the react-query cache.
    const sortedResults = [...result.value].sort(compareUserDisplayName);
    data = new SuccessResult(sortedResults);
  } else {
    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    data = result;
  }

  return { ...query, data };
};

const useApiGetUserById = (id: string) => {
  const query = useGet<User>(`/users/${id}`, ['users', id]);

  return {
    ...query,
    data: query.data as Result<User, FailResultBase>,
  };
};

const useApiCreateUser = () => {
  const mutation = usePost<Identity, CreateUser>('/users');

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

const useApiUpdateUser = () => {
  const mutation = usePutWithId<Identity, EditUser>(
    (id: string) => `/users/${id}`,
  );
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

const useApiDeleteUser = (id: string) => {
  const mutation = useDelete<void>(`/users/${id}`);

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

export {
  useApiCreateUser,
  useApiDeleteUser,
  useApiGetAllUsers,
  useApiGetUserById,
  useApiUpdateUser,
};
