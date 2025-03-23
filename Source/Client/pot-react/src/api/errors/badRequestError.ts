import { ApiError } from '../apiError';
import { ApiHeaders } from '../apiTypes';

export class BadRequestError<TData = unknown> extends ApiError<TData> {
  constructor(data?: TData, headers: ApiHeaders = {}) {
    super({ statusCode: 400, content: 'Bad Request', headers }, data);
  }
}
