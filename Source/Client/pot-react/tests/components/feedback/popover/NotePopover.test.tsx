import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test } from 'vitest';

import NotePopover from '@/components/feedback/popover/NotePopover';

describe('NotePopover', () => {
  test('renders nothing when note is empty', () => {
    const { container } = render(<NotePopover note="" />);

    expect(container.firstChild).toBeNull();
  });

  test('renders trigger and note content when note exists', async () => {
    const user = userEvent.setup();

    render(<NotePopover note="Remember to reconcile" ariaLabel="Show note" />);

    expect(
      screen.getByRole('button', { name: 'Show note' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Show note' }));

    expect(
      await screen.findByText('Remember to reconcile'),
    ).toBeInTheDocument();
    expect(await screen.findByText('Note')).toBeInTheDocument();
  });
});
