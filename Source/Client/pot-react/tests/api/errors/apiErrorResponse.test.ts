import { describe, expect, it } from 'vitest';

import type { ApiErrorResponse } from '@/api/errors/apiErrorResponse';
import {
  getBadRequestMessage,
  getConflictMessage,
  getServerErrorMessage,
  readApiErrorResponse,
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

  it('should keep the server fallback for a body it has nothing to say about', () => {
    // An unreachable application is a 502, 503 or 504 and is decided by the status, not by an
    // empty 500 body.
    const result = getServerErrorMessage({});

    expect(result).toBe('The server encountered an error. Please try again.');
  });
});

describe('getBadRequestMessage', () => {
  it('should show every message in the validation map, field and general alike', () => {
    const error: ApiErrorResponse = {
      status: 400,
      title: 'One or more validation errors occurred.',
      errors: {
        username: ['Username is required.'],
        password: ['Password is required.'],
        '': ['The request could not be read.'],
      },
    };

    const result = getBadRequestMessage(error);

    expect(result).toBe(
      'Username is required.\nPassword is required.\nThe request could not be read.',
    );
  });

  it('should show every message when one field has more than one', () => {
    const error: ApiErrorResponse = {
      status: 400,
      title: 'One or more validation errors occurred.',
      errors: {
        password: ['Password is required.', 'Password is too short.'],
      },
    };

    const result = getBadRequestMessage(error);

    expect(result).toBe('Password is required.\nPassword is too short.');
  });

  it('should fall back to the body title when the map is absent', () => {
    const error: ApiErrorResponse = {
      status: 400,
      title: 'One or more validation errors occurred.',
    };

    const result = getBadRequestMessage(error);

    expect(result).toBe('One or more validation errors occurred.');
  });
});

describe('readApiErrorResponse', () => {
  it('should read every member a problem body carries', async () => {
    const body = {
      type: 'urn:pot:problem:duplicate-value',
      title: 'Conflict',
      status: 409,
      detail: 'The account description already exists',
      traceId: '00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01',
      correlationId: '8e9f7c6d-5b4a-3f2e-1d0c-9a8b7c6d5e4f',
      instance: '/api/accounts',
    };

    const result = await readApiErrorResponse(body);

    expect(result).toEqual(body);
  });

  it('should read the validation map, the general key included', async () => {
    const body = {
      status: 400,
      title: 'One or more validation errors occurred.',
      errors: {
        email: ['Invalid email format'],
        '': ['The request could not be read.'],
      },
    };

    const result = await readApiErrorResponse(body);

    expect(result.errors).toEqual(body.errors);
  });

  it('should read a problem body that arrives as a blob', async () => {
    const body = { status: 500, detail: 'An unexpected error occurred.' };
    const blob = new Blob([JSON.stringify(body)], {
      type: 'application/problem+json',
    });

    const result = await readApiErrorResponse(blob);

    expect(result.detail).toBe('An unexpected error occurred.');
  });

  it('should keep only the members that are text', async () => {
    const body = { detail: 42, title: {}, type: [], instance: null };

    const result = await readApiErrorResponse(body);

    expect(result).toEqual({});
  });

  it('should drop a blank member so it cannot become an empty message', async () => {
    const result = await readApiErrorResponse({ status: 409, detail: '   ' });

    expect(result.detail).toBeUndefined();
  });

  it.each([
    ['a string', 'not a map'],
    ['a field that is not an array', { email: 'Invalid email format' }],
    ['a message that is not a string', { email: [1, 2] }],
    ['an array of blank messages', { email: ['  '] }],
  ])(
    'should drop an errors member that is %s',
    async (_description, errors) => {
      const result = await readApiErrorResponse({ status: 400, errors });

      expect(result.errors).toBeUndefined();
    },
  );

  it('should keep the fields of a partly unreadable map', async () => {
    const result = await readApiErrorResponse({
      status: 400,
      errors: { email: ['Invalid email format'], broken: 5 },
    });

    expect(result.errors).toEqual({ email: ['Invalid email format'] });
  });

  it.each([
    ['an html page', '<html><body><h1>502 Bad Gateway</h1></body></html>'],
    ['an empty body', ''],
    ['json that is not problem shaped', { message: 'boom' }],
    ['a json array', [{ detail: 'not a problem' }]],
  ])('should treat %s as no body at all', async (_description, body) => {
    const result = await readApiErrorResponse(body);

    expect(result).toEqual({});
  });
});
