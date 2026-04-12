import type { Row } from '@tanstack/react-table';
import { render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import type { Income } from '@/data';
import { getAdornedIncomeDescription } from '@/lib';

vi.mock('@/components/feedback', () => ({
  NotePopover: ({ note }: { note: string }) => (
    <span data-testid="note-popover">{note}</span>
  ),
}));

function createRow(
  overrides: { description?: string; note?: string | null } = {},
): Row<Income> {
  return {
    original: {
      description: 'Salary',
      note: null,
      ...overrides,
    },
  } as unknown as Row<Income>;
}

describe('incomeTableRowUtils', () => {
  test('should render income description text', () => {
    const row = createRow({ description: 'Bonus' });

    render(getAdornedIncomeDescription(row));

    expect(screen.getByText('Bonus')).toBeInTheDocument();
  });

  test('should render NotePopover when note exists', () => {
    const row = createRow({ note: 'Annual review adjustment' });

    render(getAdornedIncomeDescription(row));

    expect(screen.getByTestId('note-popover')).toHaveTextContent(
      'Annual review adjustment',
    );
  });

  test('should not render NotePopover when note is null', () => {
    const row = createRow({ note: null });

    render(getAdornedIncomeDescription(row));

    expect(screen.queryByTestId('note-popover')).not.toBeInTheDocument();
  });
});
