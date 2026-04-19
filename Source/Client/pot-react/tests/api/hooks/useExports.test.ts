import { renderHook } from '@testing-library/react';
import { useMutation } from '@tanstack/react-query';
import axios from 'axios';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { UnexpectedError } from '@/api/errors/apiErrors';
import { useApiExport } from '@/api/hooks/useExports';
import { FailResult, SuccessResult } from '@/lib';

vi.mock('@tanstack/react-query', () => ({
  useMutation: vi.fn(),
}));

vi.mock('axios', () => ({
  default: {
    get: vi.fn(),
  },
}));

describe('useExports hook composition', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useMutation).mockImplementation(
      (options: unknown) => options as never,
    );
  });

  test('configures useMutation and returns SuccessResult from export endpoint', async () => {
    const blob = new Blob(['export-data'], {
      type: 'application/octet-stream',
    });

    vi.mocked(axios.get).mockResolvedValue({
      data: blob,
      headers: { 'content-disposition': 'attachment; filename=pot.export' },
    });

    renderHook(() => useApiExport());

    expect(useMutation).toHaveBeenCalledTimes(1);

    const mutationOptions = vi.mocked(useMutation).mock.calls[0]?.[0] as
      | { mutationFn: (input: { signal?: AbortSignal }) => Promise<unknown> }
      | undefined;

    expect(mutationOptions).toBeDefined();
    if (!mutationOptions) {
      return;
    }

    const result = await mutationOptions.mutationFn({ signal: undefined });

    expect(axios.get).toHaveBeenCalledWith('/maintenance/export', {
      responseType: 'blob',
      signal: undefined,
    });

    expect(result).toBeInstanceOf(SuccessResult);

    if (result instanceof SuccessResult) {
      expect(result.value.blob).toBe(blob);
      expect(result.value.headers).toEqual({
        'content-disposition': 'attachment; filename=pot.export',
      });
    }
  });

  test('returns failure result when export request rejects', async () => {
    const failure = new FailResult(new UnexpectedError('export failed'));
    vi.mocked(axios.get).mockRejectedValue(failure);

    renderHook(() => useApiExport());

    const mutationOptions = vi.mocked(useMutation).mock.calls[0]?.[0] as
      | { mutationFn: (input: { signal?: AbortSignal }) => Promise<unknown> }
      | undefined;

    expect(mutationOptions).toBeDefined();
    if (!mutationOptions) {
      return;
    }

    const result = await mutationOptions.mutationFn({ signal: undefined });

    expect(result).toBe(failure);
  });
});
