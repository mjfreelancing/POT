import { Identity } from '@/data';

/**
 * Creates a getRowId function for DataTable that uses the rowId property from Identity-based objects.
 * This ensures selection state is maintained when data is updated and reordered.
 *
 * @template TData - The data type that extends Identity (has rowId property)
 * @returns A function that extracts the rowId as a string for table row identification
 */
function createRowIdGetter<TData extends Identity>(): (
  row: TData,
  index: number,
) => string {
  return (row: TData) => String(row.rowId);
}

export { createRowIdGetter };
