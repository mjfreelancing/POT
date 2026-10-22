import { describe, expect, it } from 'vitest';

import type { ApiErrorResponse } from '@/api/errors/apiErrorResponse';
import {
  getConflictMessage,
  getServerErrorMessage,
} from '@/api/errors/apiErrorResponse';

describe('getConflictMessage', () => {
  it('should use the detail the server wrote for a data conflict', () => {
    const error: ApiErrorResponse = {
      status: 409,
      type: 'urn:pot:problem:data-conflict',
      detail: 'The request conflicts with the current state of the data.',
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

  it('should show the refresh message when the problem type says the entity tag is stale', () => {
    const error: ApiErrorResponse = {
      status: 409,
      type: 'urn:pot:problem:stale-entity-tag',
      detail:
        'The entity tag for the Account does not match the current record',
    };

    const result = getConflictMessage(error);

    expect(result).toBe(
      'A conflicting update has been performed by another user. Refresh and try again.',
    );
  });

  it('should use the server detail for a duplicate-value conflict', () => {
    const error: ApiErrorResponse = {
      status: 409,
      type: 'urn:pot:problem:duplicate-value',
      detail: 'The account description already exists',
    };

    const result = getConflictMessage(error);

    expect(result).toBe(error.detail);
  });
});

describe('getServerErrorMessage', () => {
  it('should show the detail the server wrote for a 500', () => {
    const error: ApiErrorResponse = {
      status: 500,
      title: 'An error occurred while processing your request.',
      detail: 'An unexpected error occurred.',
    };

    const result = getServerErrorMessage(error);

    expect(result).toBe('An unexpected error occurred.');
  });

  it('should keep the fallback when the body carries no detail or title', () => {
    const error: ApiErrorResponse = {
      status: 500,
      traceId: '00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01',
    };

    const result = getServerErrorMessage(error);

    expect(result).toBe('The server encountered an error. Please try again.');
  });
});
