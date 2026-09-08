import type { RowData } from '@tanstack/react-table';

import type { AppRow } from '@/components/table/tableFeatures';

function createGenericRow<TData extends RowData>(
  defaults: TData,
  overrides: Partial<TData> = {},
): AppRow<TData> {
  return {
    original: {
      ...defaults,
      ...overrides,
    },
  } as unknown as AppRow<TData>;
}

export { createGenericRow };
