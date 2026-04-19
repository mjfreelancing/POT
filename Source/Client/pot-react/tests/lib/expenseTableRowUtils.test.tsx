import type { Row } from '@tanstack/react-table';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test } from 'vitest';

import type { Expense } from '@/data';
import { getAdornedExpenseDescription } from '@/lib';

import { createGenericRow } from '../shared/rows/genericRowBuilder';

function createRow(
  overrides: { description?: string; note?: string | null } = {},
): Row<Expense> {
  return createGenericRow<Expense>(
    {
      description: 'Rent',
      note: null,
    } as Expense,
    overrides,
  );
}

describe('expenseTableRowUtils', () => {
  test('should render expense description text', () => {
    const row = createRow({ description: 'Internet' });

    render(getAdornedExpenseDescription(row));

    expect(screen.getByText('Internet')).toBeInTheDocument();
  });

  test('should render NotePopover when note exists', async () => {
    const user = userEvent.setup();
    const row = createRow({ note: 'Paid via direct debit' });

    render(getAdornedExpenseDescription(row));

    await user.click(screen.getByRole('button', { name: 'Show note' }));

    expect(screen.getByText('Note')).toBeInTheDocument();
    expect(screen.getByText('Paid via direct debit')).toBeInTheDocument();
  });

  test('should not render NotePopover when note is null', () => {
    const row = createRow({ note: null });

    render(getAdornedExpenseDescription(row));

    expect(
      screen.queryByRole('button', { name: 'Show note' }),
    ).not.toBeInTheDocument();
  });
});
