import { FailResultBase } from '@/lib/result/failResultBase';

import { ApiResponse } from './apiTypes';

export class ApiError<TData = unknown> extends FailResultBase {
  public readonly apiResponse: ApiResponse;
  public readonly data?: TData; // error specific (contextual) data

  constructor(apiResponse: ApiResponse, data?: TData) {
    // Not using 'code' or 'description' on super
    super('ApiError');

    this.apiResponse = apiResponse;
    this.data = data;
  }
}
