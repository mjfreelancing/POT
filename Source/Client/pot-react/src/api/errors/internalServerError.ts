import { ApiError } from '../apiError';
import { ApiHeaders } from '../apiTypes';

export class InternalServerError<TData = unknown> extends ApiError<TData> {
  constructor(data?: TData, headers: ApiHeaders = {}) {
    super({ statusCode: 500, content: 'Internal Server Error', headers }, data);
  }
}
