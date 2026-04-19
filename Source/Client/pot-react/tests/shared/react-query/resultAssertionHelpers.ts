import { expect } from 'vitest';

import { FailResult, FailResultBase, SuccessResult } from '@/lib';

function expectSuccessResult<T>(
  result: SuccessResult<T> | FailResult<FailResultBase>,
  expected: T,
): void {
  expect(result).toBeInstanceOf(SuccessResult);
  expect(result.success).toBe(true);

  if (result.success) {
    expect(result.value).toEqual(expected);
  }
}

function expectFailResult<E extends FailResultBase>(
  result: SuccessResult<unknown> | FailResult<E>,
  errorType: new (description: string) => E,
): void {
  expect(result).toBeInstanceOf(FailResult);
  expect(result.success).toBe(false);

  if (!result.success) {
    expect(result.error).toBeInstanceOf(errorType);
  }
}

export { expectFailResult, expectSuccessResult };
