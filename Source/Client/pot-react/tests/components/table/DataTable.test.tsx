import type { ColumnDef } from '@tanstack/react-table';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import DataTable from '@/components/table/DataTable';
import DataTableColumnHeader from '@/components/table/DataTableColumnHeader';

// Renders the REAL DataTable (react-table through the shadcn-style wrapper),
// driving sorting, row selection and custom getRowId through the actual DOM.
// This is the Inc 9 (react-table 8 -> 9) pre-work guard: it locks the v8
// behavior that the migration must preserve.

type TestRow = {
  rowId: string;
  description: string;
  amount: number;
};

const columns: ColumnDef<TestRow>[] = [
  {
    id: 'description',
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    cell: ({ row }) => <span>{row.original.description}</span>,
  },
  {
    id: 'amount',
    accessorKey: 'amount',
    header: () => <div className="uppercase">Amount</div>,
    enableSorting: false,
    cell: ({ row }) => <span>{row.original.amount}</span>,
  },
];

const rows: TestRow[] = [
  { rowId: 'row-2', description: 'Bravo', amount: 200 },
  { rowId: 'row-1', description: 'Alpha', amount: 100 },
  { rowId: 'row-3', description: 'Charlie', amount: 300 },
];

const getBodyRows = () => screen.getAllByRole('row').slice(1);

const getFirstColumnTexts = () =>
  getBodyRows().map(row =>
    within(row).getAllByRole('cell')[0].textContent?.trim(),
  );

describe('DataTable (real render)', () => {
  test('renders rows and the empty state', () => {
    const { rerender } = render(
      <DataTable<TestRow, unknown> columns={columns} data={rows} />,
    );

    expect(screen.getByRole('columnheader', { name: /description/i })).toBeInTheDocument();
    expect(getFirstColumnTexts()).toEqual(['Bravo', 'Alpha', 'Charlie']);

    rerender(<DataTable<TestRow, unknown> columns={columns} data={[]} />);
    expect(screen.getByText('No results.')).toBeInTheDocument();
  });

  test('sorts rows when a sortable header is clicked (asc then desc)', async () => {
    const user = userEvent.setup();

    render(<DataTable<TestRow, unknown> columns={columns} data={rows} />);

    const descriptionHeader = screen.getByRole('columnheader', {
      name: /description/i,
    });
    const sortButton = within(descriptionHeader).getByRole('button', {
      name: /description/i,
    });

    // First click -> ascending.
    await user.click(sortButton);
    expect(getFirstColumnTexts()).toEqual(['Alpha', 'Bravo', 'Charlie']);

    // Second click -> descending.
    await user.click(sortButton);
    expect(getFirstColumnTexts()).toEqual(['Charlie', 'Bravo', 'Alpha']);
  });

  test('selects rows by a custom getRowId and reports them on change', async () => {
    const user = userEvent.setup();
    const onSelectionChange = vi.fn();

    render(
      <DataTable<TestRow, unknown>
        columns={columns}
        data={rows}
        enableRowSelection
        getRowId={row => row.rowId}
        onSelectionChange={onSelectionChange}
      />,
    );

    // The row checkbox aria-label uses the resolved row id (custom getRowId).
    await user.click(screen.getByRole('checkbox', { name: 'Select row row-2' }));
    expect(onSelectionChange).toHaveBeenLastCalledWith([
      expect.objectContaining({ rowId: 'row-2', description: 'Bravo' }),
    ]);

    // Selection is stable across re-renders because it keys on rowId, not index.
    await user.click(screen.getByRole('checkbox', { name: 'Select row row-1' }));
    expect(onSelectionChange).toHaveBeenLastCalledWith([
      expect.objectContaining({ rowId: 'row-2' }),
      expect.objectContaining({ rowId: 'row-1' }),
    ]);
  });
});
