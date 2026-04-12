import type { Row } from '@tanstack/react-table';
import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import type { Expense } from '@/data';
import { getAdornedExpenseDescription } from '@/lib';

vi.mock('@/components/feedback', () => ({
  NotePopover: ({ note }: { note: string }) => (
    <span data-testid="note-popover">{note}</span>
  ),
}));

function createRow(
  overrides: { description?: string; note?: string | null } = {},
): Row<Expense> {
  return {
    original: {
      description: 'Rent',
      note: null,
      ...overrides,
    },
  } as unknown as Row<Expense>;
}

describe('expenseTableRowUtils', () => {
  test('should render expense description text', () => {
    const row = createRow({ description: 'Internet' });

    render(getAdornedExpenseDescription(row));

    expect(screen.getByText('Internet')).toBeInTheDocument();
  });

  test('should render NotePopover when note exists', () => {
    const row = createRow({ note: 'Paid via direct debit' });

    render(getAdornedExpenseDescription(row));

    expect(screen.getByTestId('note-popover')).toHaveTextContent(
      'Paid via direct debit',
    );
  });

  test('should not render NotePopover when note is null', () => {
    const row = createRow({ note: null });

    render(getAdornedExpenseDescription(row));

    expect(screen.queryByTestId('note-popover')).not.toBeInTheDocument();
  });
});
