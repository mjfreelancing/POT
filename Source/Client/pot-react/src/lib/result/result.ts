import { FailResultBase } from './failResultBase';

export type Result<TSuccess, TFail extends FailResultBase> =
  | { success: true; value: TSuccess }
  | { success: false; error: TFail };

export class ResultFactory {
  static success<TSuccess>(value: TSuccess): Result<TSuccess, never> {
    return { success: true, value };
  }

  static fail<TFail extends FailResultBase>(
    error: TFail,
  ): Result<never, TFail> {
    return { success: false, error };
  }
}

export function isSuccess<TSuccess, TFail extends FailResultBase>(
  result: Result<TSuccess, TFail>,
): result is { success: true; value: TSuccess } {
  return result.success;
}

export function isFail<TSuccess, TFail extends FailResultBase>(
  result: Result<TSuccess, TFail>,
): result is { success: false; error: TFail } {
  return !result.success;
}
