import { faker } from '@faker-js/faker';
import { beforeEach, describe, expect, test } from 'vitest';

import { FailResultBase } from './failResultBase';
import { FailResult, Result, SuccessResult } from './result';

class DummyError extends FailResultBase {
  constructor(type: string, code: string, description: string) {
    super(type, code, description);
  }
}

describe('Result Type', () => {
  let errorType: string;
  let errorCode: string;
  let errorDescription: string;
  let dummyError: DummyError;

  beforeEach(() => {
    errorType = faker.word.sample();
    errorCode = faker.string.alphanumeric(8);
    errorDescription = faker.lorem.sentence();
    dummyError = new DummyError(errorType, errorCode, errorDescription);
  });

  test('should create a success result', () => {
    // Deliberately using FailResultBase instead of DummyError since the error could be any derived type.
    const successValue = faker.lorem.sentence();
    const result: Result<string, FailResultBase> = new SuccessResult(
      successValue,
    );

    expect(result.success).toBe(true);
    expect(result.value).toBe(successValue);
  });

  test('should create a failure result', () => {
    // Deliberately using FailResultBase instead of DummyError since the error could be any derived type.
    const result: Result<string, FailResultBase> = new FailResult(dummyError);

    expect(result.success).toBe(false);

    expect(result.error).toBe(dummyError);
    expect(result.error).toBeInstanceOf(DummyError);
    expect(result.error).toBeInstanceOf(FailResultBase);
    expect(result.error.type).toBe(errorType);
    expect(result.error.code).toBe(errorCode);
    expect(result.error.description).toBe(errorDescription);
  });
});
