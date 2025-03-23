import { FailResultBase } from './failResultBase';
import { isFail, isSuccess, ResultFactory } from './result';

class DummyError extends FailResultBase {
  constructor() {
    super(undefined, 'TEST', 'Test Error');
  }
}

describe('Result Type', () => {
  it('should create a success result', () => {
    const result = ResultFactory.success('Success!');

    expect(isSuccess(result)).toBe(true);
    expect(isFail(result)).toBe(false);

    if (isSuccess(result)) {
      expect(result.value).toBe('Success!');
    }
  });

  it('should create a failure result', () => {
    const error = new DummyError();
    const result = ResultFactory.fail(error);

    expect(isSuccess(result)).toBe(false);
    expect(isFail(result)).toBe(true);

    if (isFail(result)) {
      expect(result.error).toBe(error);
      expect(result.error.type).toBe(undefined);
      expect(result.error.code).toBe('TEST');
      expect(result.error.description).toBe('Test Error');
    }
  });
});
