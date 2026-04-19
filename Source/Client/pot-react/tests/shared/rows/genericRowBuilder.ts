import type { Row } from '@tanstack/react-table';

function createGenericRow<TData extends object>(
  defaults: TData,
  overrides: Partial<TData> = {},
): Row<TData> {
  return {
    original: {
      ...defaults,
      ...overrides,
    },
  } as unknown as Row<TData>;
}

export { createGenericRow };
