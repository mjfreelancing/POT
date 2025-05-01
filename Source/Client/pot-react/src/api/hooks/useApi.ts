import { useMutation, useQuery } from '@tanstack/react-query';
import axios, { AxiosResponse } from 'axios';

axios.defaults.baseURL = 'http://localhost:5241/api';
axios.defaults.timeout = 3000;

// axios.defaults.headers.common['Authorization'] = 'Bearer your-token';

// // Request Interceptor
// axios.interceptors.request.use((config: AxiosRequestConfig) => {
//   if (token) {
//     config.headers = config.headers || {}; // Ensure headers object exists
//     config.headers.Authorization = `Bearer ${token}`;
//   }
//   return config;
// });

// // Response Interceptor
// axios.interceptors.response.use(
//   res => res,
//   (error: AxiosError) => {
//     if (error.response) {
//       const { data, status } = error.response;
//       switch (status) {
//         case 400:
//           console.error(data);
//           break;
//         case 401:
//           console.error('Unauthorized');
//           break;
//         case 404:
//           console.error('Not Found');
//           break;
//         case 500:
//           console.error('Server Error');
//           break;
//       }
//     }
//     return Promise.reject(error);
//   },
// );

const responseData = <TResponse>(
  response: AxiosResponse<TResponse>,
): TResponse => {
  return response.data;
};

export const useGet = <TResponse>(url: string, queryKey: string[]) => {
  return useQuery({
    queryKey,
    queryFn: async ({ signal }) => {
      return axios.get<TResponse>(url, { signal }).then(responseData);
    },
  });
};

export const usePost = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: {
      data: TData;
      signal?: AbortSignal;
    }) => {
      return axios.post<TResponse>(url, data, { signal }).then(responseData);
    },
  });
};

export const usePut = <TResponse, TData>(url: string) => {
  return useMutation({
    mutationFn: async ({
      data,
      signal,
    }: {
      data: TData;
      signal?: AbortSignal;
    }) => {
      return axios.put<TResponse>(url, data, { signal }).then(responseData);
    },
  });
};

export const useDelete = <TResponse>(url: string) => {
  return useMutation({
    mutationFn: async ({ signal }: { signal?: AbortSignal }) => {
      return axios.delete<TResponse>(url, { signal }).then(responseData);
    },
  });
};
