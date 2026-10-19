import { describe, expect, it } from 'vitest';

import type { ApiErrorResponse } from '@/api/errors/apiErrorResponse';
import { getConflictMessage } from '@/api/errors/apiErrorResponse';

describe('getConflictMessage', () => {
  it('should use the detail for a server-generated conflict with an empty property name', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'The request conflicts with the current state of the data.',
      errors: [
        {
          propertyName: '',
          errorCode: 'Conflict',
          attemptedValue: null,
          errorMessage:
            'The request conflicts with the current state of the data.',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe(error.detail);
  });

  it('should use the detail when a conflict detail has no property name', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'The request conflicts with the current state of the data.',
      errors: [
        {
          errorCode: 'Conflict',
          errorMessage:
            'The request conflicts with the current state of the data.',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe(error.detail);
  });

  it('should use the detail when a conflict has no errors', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'Resource already exists',
    };

    const result = getConflictMessage(error);

    expect(result).toBe('Resource already exists');
  });

  it('should fall back to a generic message when there is no detail', () => {
    const error: ApiErrorResponse = { status: 409 };

    const result = getConflictMessage(error);

    expect(result).toBe('A conflict error occurred');
  });

  it('should describe an etag conflict', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'The entity tag does not match',
      errors: [
        {
          propertyName: 'Etag',
          errorCode: 'Conflict',
          attemptedValue: 7,
          errorMessage:
            'The entity tag for the account does not match the current record',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe(
      'A conflicting update has been performed by another user. Refresh and try again.',
    );
  });

  it('should describe a field conflict with the attempted value', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'Resource already exists',
      errors: [
        {
          propertyName: 'name',
          errorCode: 'Conflict',
          attemptedValue: 'Savings',
          errorMessage: 'Name already exists',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe(
      "The 'name' conflicts with another record that has the same value 'Savings'",
    );
  });

  it('should not print null when a field conflict has no attempted value', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'Resource already exists',
      errors: [
        {
          propertyName: 'name',
          errorCode: 'Conflict',
          attemptedValue: null,
          errorMessage: 'Name already exists',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe(
      "The 'name' conflicts with another record that has the same value ''",
    );
  });
});
