import { FailResultBase } from '@/lib/result/failResultBase';

import { ApiResponse } from './apiTypes';

export class ApiError<TData = unknown> extends FailResultBase {
  constructor(
    public apiResponse: ApiResponse,
    public data?: TData, // Error specific (contextual) data.
  ) {
    // Not using 'code' or 'description' on super
    super('ApiError');
  }
}
