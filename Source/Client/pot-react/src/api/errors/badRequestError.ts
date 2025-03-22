import { ApiError } from '../apiError';
import { ApiHeaders } from '../apiTypes';

/**
 * Represents a 400 Bad Request error.
 * @template TData - The type of the error-related data.
 */
export class BadRequestError<TData = unknown> extends ApiError<TData> {
  /**
   * Creates an instance of BadRequestError.
   * @param data - Optional additional data related to the error.
   * @param headers - Optional HTTP headers for the error response.
   */
  constructor(data?: TData, headers: ApiHeaders = {}) {
    super({ statusCode: 400, content: 'Bad Request', headers }, data);
  }
}
