import { FailResultBase } from './failResultBase';
import { FailResult, Result, SuccessResult } from './result';

class DummyError extends FailResultBase {
  constructor() {
    super(undefined, 'TEST', 'Test Error');
  }
}

describe('Result Type', () => {
  it('should create a success result', () => {
    const result: Result<string, DummyError> = new SuccessResult('Success!');

    expect(result.success).toBe(true);

    if (result.success) {
      expect(result.value).toBe('Success!');
    } else {
      throw new Error('Expected success but got failure');
    }
  });

  it('should create a failure result', () => {
    const error = new DummyError();
    const result: Result<string, DummyError> = new FailResult(error);

    expect(result.success).toBe(false);

    if (!result.success) {
      expect(result.error).toBe(error);
      expect(result.error.type).toBe(undefined);
      expect(result.error.code).toBe('TEST');
      expect(result.error.description).toBe('Test Error');
    } else {
      throw new Error('Expected failure but got success');
    }
  });
});
