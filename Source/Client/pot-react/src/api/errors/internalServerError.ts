import { ApiError } from '../apiError';
import { ApiHeaders } from '../apiTypes';

/**
 * Represents a 500 Internal Server Error.
 * @template TData - The type of the error-related data.
 */
export class InternalServerError<TData = unknown> extends ApiError<TData> {
  /**
   * Creates an instance of InternalServerError.
   * @param data - Optional additional data related to the error.
   * @param headers - Optional HTTP headers for the error response.
   */
  constructor(data?: TData, headers: ApiHeaders = {}) {
    super({ statusCode: 500, content: 'Internal Server Error', headers }, data);
  }
}
