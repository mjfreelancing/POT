import { InvalidOperationError } from '../errors/invalidOperationError';
import { FailResultBase } from './failResultBase';
import { Result } from './result';

class DummyError extends FailResultBase {
  constructor() {
    super(undefined, 'TEST', 'Test Error');
  }
}

describe('Result Class', () => {
  it('should create a success result', () => {
    const result = Result.success('Success!');

    expect(result.isSuccess()).toBe(true);
    expect(result.isFail()).toBe(false);
    expect(result.getValue()).toBe('Success!');
  });

  it('should throw InvalidOperationError when trying to get error from a success result', () => {
    const result = Result.success('Success!');

    expect(() => result.getError()).toThrow(
      new InvalidOperationError('Cannot get error from a success result'),
    );
  });

  it('should create a failure result', () => {
    const error = new DummyError();
    const result = Result.fail(error);

    expect(result.isSuccess()).toBe(false);
    expect(result.isFail()).toBe(true);

    const resultError = result.getError();
    expect(resultError).toBe(error);
    expect(resultError.type).toBe(undefined);
    expect(resultError.code).toBe('TEST');
    expect(resultError.description).toBe('Test Error');
  });

  it('should throw InvalidOperationError when trying to get value from a failed result', () => {
    const error = new DummyError();
    const result = Result.fail(error);

    expect(() => result.getValue()).toThrow(
      new InvalidOperationError('Cannot get value from a failed result'),
    );
  });
});
