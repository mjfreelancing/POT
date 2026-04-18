import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { describe, expect, test, vi } from 'vitest';

import NotePopover from '@/components/feedback/popover/NotePopover';

vi.mock('@/components/ui/popover', () => ({
  Popover: ({ children }: { children: ReactNode }) => <>{children}</>,
  PopoverTrigger: ({ children }: { children: ReactNode }) => <>{children}</>,
  PopoverContent: ({ children }: { children: ReactNode }) => (
    <div>{children}</div>
  ),
}));

describe('NotePopover', () => {
  test('renders nothing when note is empty', () => {
    const { container } = render(<NotePopover note="" />);

    expect(container.firstChild).toBeNull();
  });

  test('renders trigger and note content when note exists', () => {
    render(<NotePopover note="Remember to reconcile" ariaLabel="Show note" />);

    expect(
      screen.getByRole('button', { name: 'Show note' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Remember to reconcile')).toBeInTheDocument();
    expect(screen.getByText('Note')).toBeInTheDocument();
  });
});
