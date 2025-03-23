import axios, { AxiosResponse } from 'axios';
// import axios, { AxiosError, AxiosResponse, AxiosRequestConfig } from 'axios';
// import token from './somewhere';

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

export class ApiBase {
  protected async get<TResponse>(
    url: string,
    signal: AbortSignal,
  ): Promise<TResponse> {
    return axios
      .get<TResponse>(url, { signal: signal })
      .then(this.responseData);
  }

  protected async post<TResponse, TData>(
    url: string,
    data: TData,
    signal?: AbortSignal,
  ): Promise<TResponse> {
    return axios
      .post<TResponse>(url, data, { signal: signal })
      .then(this.responseData);
  }

  private responseData<TResponse>(
    response: AxiosResponse<TResponse>,
  ): TResponse {
    return response.data;
  }
}
