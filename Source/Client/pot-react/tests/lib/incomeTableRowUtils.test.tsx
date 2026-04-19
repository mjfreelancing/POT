import type { Row } from '@tanstack/react-table';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test } from 'vitest';

import type { Income } from '@/data';
import { getAdornedIncomeDescription } from '@/lib';

import { createGenericRow } from '../shared/rows/genericRowBuilder';

function createRow(
  overrides: { description?: string; note?: string | null } = {},
): Row<Income> {
  return createGenericRow<Income>(
    {
      description: 'Salary',
      note: null,
    } as Income,
    overrides,
  );
}

describe('incomeTableRowUtils', () => {
  test('should render income description text', () => {
    const row = createRow({ description: 'Bonus' });

    render(getAdornedIncomeDescription(row));

    expect(screen.getByText('Bonus')).toBeInTheDocument();
  });

  test('should render NotePopover when note exists', async () => {
    const user = userEvent.setup();
    const row = createRow({ note: 'Annual review adjustment' });

    render(getAdornedIncomeDescription(row));

    await user.click(screen.getByRole('button', { name: 'Show note' }));

    expect(screen.getByText('Note')).toBeInTheDocument();
    expect(screen.getByText('Annual review adjustment')).toBeInTheDocument();
  });

  test('should not render NotePopover when note is null', () => {
    const row = createRow({ note: null });

    render(getAdornedIncomeDescription(row));

    expect(
      screen.queryByRole('button', { name: 'Show note' }),
    ).not.toBeInTheDocument();
  });
});
