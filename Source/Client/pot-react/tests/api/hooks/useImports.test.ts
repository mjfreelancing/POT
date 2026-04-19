import { useMutation } from '@tanstack/react-query';
import { renderHook } from '@testing-library/react';
import axios from 'axios';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useApiImport } from '@/api/hooks/useImports';
import { FailResult, SuccessResult } from '@/lib';

vi.mock('@tanstack/react-query', () => ({
  useMutation: vi.fn(),
}));

vi.mock('axios', () => ({
  default: {
    post: vi.fn(),
  },
}));

describe('useImports hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useMutation).mockImplementation(
      (options: unknown) => options as never,
    );
  });

  test('configures useMutation and returns SuccessResult from import endpoint', async () => {
    vi.mocked(axios.post).mockResolvedValue({
      data: { imported: 7 },
    });

    renderHook(() => useApiImport());

    expect(useMutation).toHaveBeenCalledTimes(1);

    const mutationOptions = vi.mocked(useMutation).mock.calls[0]?.[0] as
      | {
          mutationFn: (input: {
            file: File;
            signal?: AbortSignal;
          }) => Promise<unknown>;
        }
      | undefined;

    expect(mutationOptions).toBeDefined();
    if (!mutationOptions) {
      return;
    }

    const file = new File(['csv-data'], 'input.csv', { type: 'text/csv' });

    const result = await mutationOptions.mutationFn({
      file,
      signal: undefined,
    });

    expect(axios.post).toHaveBeenCalledTimes(1);
    expect(vi.mocked(axios.post).mock.calls[0]?.[0]).toBe(
      '/maintenance/import',
    );

    const formDataArg = vi.mocked(axios.post).mock.calls[0]?.[1] as FormData;
    expect(formDataArg).toBeInstanceOf(FormData);
    expect(formDataArg.get('file')).toBe(file);

    expect(vi.mocked(axios.post).mock.calls[0]?.[2]).toEqual({
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      signal: undefined,
    });

    expect(result).toBeInstanceOf(SuccessResult);

    if (result instanceof SuccessResult) {
      expect(result.value).toEqual({ imported: 7 });
    }
  });

  test('returns failure result when import request rejects', async () => {
    const failure = new FailResult(new UnexpectedError('import failed'));
    vi.mocked(axios.post).mockRejectedValue(failure);

    renderHook(() => useApiImport());

    const mutationOptions = vi.mocked(useMutation).mock.calls[0]?.[0] as
      | {
          mutationFn: (input: {
            file: File;
            signal?: AbortSignal;
          }) => Promise<unknown>;
        }
      | undefined;

    expect(mutationOptions).toBeDefined();
    if (!mutationOptions) {
      return;
    }

    const file = new File(['csv-data'], 'input.csv', { type: 'text/csv' });

    const result = await mutationOptions.mutationFn({
      file,
      signal: undefined,
    });

    expect(result).toBe(failure);
  });
});
