import { FailResultBase } from '@/lib/result/failResultBase';

import { ApiResponse } from './apiTypes';

/**
 * Represents a base class for API errors.
 * @template TData - The type of the error-related data.
 */
export class ApiError<TData = unknown> extends FailResultBase {
  /** The API response associated with the error. */
  public readonly apiResponse: ApiResponse;

  /** Optional additional data related to the error. */
  public readonly data?: TData;

  /**
   * Creates an instance of ApiError.
   * @param apiResponse - The API response structure containing status code, headers, and content.
   * @param data - Optional additional data related to the error.
   */
  constructor(apiResponse: ApiResponse, data?: TData) {
    // Not using 'code' or 'description' on super
    super('ApiError');

    this.apiResponse = apiResponse;
    this.data = data;
  }
}
