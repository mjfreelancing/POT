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

  it('should use the server detail for a conflict that names a field', () => {
    const error: ApiErrorResponse = {
      status: 409,
      detail: 'The account description already exists',
      errors: [
        {
          propertyName: 'Description',
          errorCode: 'Conflict',
          errorMessage: 'The account description already exists',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe(error.detail);
  });

  it('should fall back to the field error messages when there is no detail', () => {
    const error: ApiErrorResponse = {
      status: 409,
      errors: [
        {
          propertyName: 'name',
          errorCode: 'Conflict',
          errorMessage: 'Name already exists',
        },
      ],
    };

    const result = getConflictMessage(error);

    expect(result).toBe('Name already exists');
  });
});
