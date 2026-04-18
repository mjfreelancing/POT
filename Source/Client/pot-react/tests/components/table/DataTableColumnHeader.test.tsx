import type { Column } from '@tanstack/react-table';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import DataTableColumnHeader from '@/components/table/DataTableColumnHeader';

type SortState = false | 'asc' | 'desc';

type ColumnMock = {
  getCanSort: ReturnType<typeof vi.fn>;
  getIsSorted: ReturnType<typeof vi.fn>;
  toggleSorting: ReturnType<typeof vi.fn>;
  clearSorting: ReturnType<typeof vi.fn>;
};

const createColumnMock = (sortState: SortState, canSort = true): ColumnMock => ({
  getCanSort: vi.fn().mockReturnValue(canSort),
  getIsSorted: vi.fn().mockReturnValue(sortState),
  toggleSorting: vi.fn(),
  clearSorting: vi.fn(),
});

describe('DataTableColumnHeader', () => {
  test('renders plain title when column is not sortable', () => {
    const columnMock = createColumnMock(false, false);

    render(
      <DataTableColumnHeader
        column={columnMock as unknown as Column<unknown, unknown>}
        title="Amount"
      />,
    );

    expect(screen.getByText('Amount')).toBeInTheDocument();
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  test('starts ascending when current sort is unsorted', async () => {
    const user = userEvent.setup();
    const columnMock = createColumnMock(false);

    render(
      <DataTableColumnHeader
        column={columnMock as unknown as Column<unknown, unknown>}
        title="Amount"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Amount' }));

    expect(columnMock.toggleSorting).toHaveBeenCalledWith(false);
    expect(columnMock.clearSorting).not.toHaveBeenCalled();
  });

  test('switches to descending when current sort is ascending', async () => {
    const user = userEvent.setup();
    const columnMock = createColumnMock('asc');

    render(
      <DataTableColumnHeader
        column={columnMock as unknown as Column<unknown, unknown>}
        title="Amount"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Amount' }));

    expect(columnMock.toggleSorting).toHaveBeenCalledWith(true);
    expect(columnMock.clearSorting).not.toHaveBeenCalled();
  });

  test('clears sorting when current sort is descending', async () => {
    const user = userEvent.setup();
    const columnMock = createColumnMock('desc');

    render(
      <DataTableColumnHeader
        column={columnMock as unknown as Column<unknown, unknown>}
        title="Amount"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Amount' }));

    expect(columnMock.clearSorting).toHaveBeenCalledTimes(1);
    expect(columnMock.toggleSorting).not.toHaveBeenCalled();
  });
});
