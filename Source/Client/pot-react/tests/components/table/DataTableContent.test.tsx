import type { Row } from '@tanstack/react-table';
import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';

import DataTableContent from '@/components/table/DataTableContent';

type TestRow = {
  id: string;
  name: string;
};

type TableMock = {
  getRowModel: () => {
    rows: Array<{
      id: string;
      getIsSelected: () => boolean;
      getVisibleCells: () => Array<{
        id: string;
        column: {
          columnDef: {
            cell: (context: { value: string }) => string;
          };
        };
        getContext: () => {
          value: string;
        };
      }>;
    }>;
  };
};

const createTableMock = (rows: TableMock['getRowModel'] extends () => infer R ? R['rows'] : never): TableMock => ({
  getRowModel: () => ({
    rows,
  }),
});

describe('DataTableContent', () => {
  test('renders row content and highlight class when highlight filter matches', () => {
    const rowValue = 'Alpha';
    const tableMock = createTableMock([
      {
        id: 'row-1',
        getIsSelected: () => true,
        getVisibleCells: () => [
          {
            id: 'cell-1',
            column: {
              columnDef: {
                cell: (context: { value: string }) => context.value,
              },
            },
            getContext: () => ({
              value: rowValue,
            }),
          },
        ],
      },
    ]);

    render(
      <table>
        <DataTableContent<TestRow>
          table={tableMock as never}
          tableColumns={[{ id: 'name' } as never]}
          highlightRowFilter={(row: Row<TestRow>) => row.id === 'row-1'}
          highlightClassName="bg-amber-100"
        />
      </table>,
    );

    const rowText = screen.getByText(rowValue);
    const rowElement = rowText.closest('tr');

    expect(rowElement).toHaveAttribute('data-state', 'selected');
    expect(rowElement).toHaveClass('bg-amber-100');
  });

  test('renders empty state row when table has no rows', () => {
    const tableMock = createTableMock([]);

    render(
      <table>
        <DataTableContent<TestRow>
          table={tableMock as never}
          tableColumns={[{ id: 'name' } as never, { id: 'status' } as never]}
        />
      </table>,
    );

    const noResultsCell = screen.getByText('No results.');

    expect(noResultsCell).toBeInTheDocument();
    expect(noResultsCell.closest('td')).toHaveAttribute('colspan', '2');
  });
});
