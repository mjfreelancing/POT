import { InvalidOperationError } from '../errors/invalidOperationError';
import { FailResultBase } from './failResultBase';

/**
 * Represents the result of an operation that can either succeed or fail.
 * @template TSuccess - The type of the successful result value.
 * @template TFail - The type of the failure result, extending FailResultBase.
 */
export class Result<TSuccess, TFail extends FailResultBase> {
  /**
   * Creates an instance of Result.
   * @param value - The successful result value, if applicable.
   * @param error - The failure result, if applicable.
   */
  private constructor(
    private readonly value?: TSuccess,
    private readonly error?: TFail,
  ) {}

  /**
   * Creates a successful result.
   * @param value - The successful result value.
   * @returns An instance of Result representing success.
   */
  static success<TSuccess>(value: TSuccess): Result<TSuccess, never> {
    return new Result<TSuccess, never>(value, undefined as never);
  }

  /**
   * Creates a failed result.
   * @param error - The failure result.
   * @returns An instance of Result representing failure.
   */
  static fail<TFail extends FailResultBase>(
    error: TFail,
  ): Result<never, TFail> {
    return new Result<never, TFail>(undefined as never, error);
  }

  /**
   * Determines if the result represents success.
   * @returns True if the result is successful, otherwise false.
   */
  isSuccess(): boolean {
    return this.value !== undefined;
  }

  /**
   * Determines if the result represents failure.
   * @returns True if the result is a failure, otherwise false.
   */
  isFail(): boolean {
    return this.error !== undefined;
  }

  /**
   * Gets the successful result value.
   * @returns The success value.
   * @throws An InvalidOperationError if the result is a failure.
   */
  getValue(): TSuccess {
    if (!this.isSuccess()) {
      throw new InvalidOperationError('Cannot get value from a failed result');
    }

    return this.value as TSuccess;
  }

  /**
   * Gets the failure result.
   * @returns The failure result.
   * @throws An InvalidOperationError if the result is a success.
   */
  getError(): TFail {
    if (!this.isFail()) {
      throw new InvalidOperationError('Cannot get error from a success result');
    }

    return this.error as TFail;
  }
}
