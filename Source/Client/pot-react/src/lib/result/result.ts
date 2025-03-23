import { InvalidOperationError } from '../errors/invalidOperationError';
import { FailResultBase } from './failResultBase';

export class Result<TSuccess, TFail extends FailResultBase> {
  private constructor(
    private readonly value?: TSuccess,
    private readonly error?: TFail,
  ) {}

  static success<TSuccess>(value: TSuccess): Result<TSuccess, never> {
    return new Result<TSuccess, never>(value, undefined as never);
  }

  static fail<TFail extends FailResultBase>(
    error: TFail,
  ): Result<never, TFail> {
    return new Result<never, TFail>(undefined as never, error);
  }

  isSuccess(): boolean {
    return this.value !== undefined;
  }

  isFail(): boolean {
    return this.error !== undefined;
  }

  getValue(): TSuccess {
    if (!this.isSuccess()) {
      throw new InvalidOperationError('Cannot get value from a failed result');
    }

    return this.value as TSuccess;
  }

  getError(): TFail {
    if (!this.isFail()) {
      throw new InvalidOperationError('Cannot get error from a success result');
    }

    return this.error as TFail;
  }
}
