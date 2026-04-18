import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

describe('ErrorSheet', () => {
  test('renders title and multiline description', () => {
    render(
      <ErrorSheet
        title="Save failed"
        description={'First line\nSecond line'}
        onDismiss={vi.fn()}
      />,
    );

    expect(screen.getByText('Save failed')).toBeInTheDocument();
    expect(screen.getByText('First line')).toBeInTheDocument();
    expect(screen.getByText('Second line')).toBeInTheDocument();
  });

  test('calls onDismiss when dismiss button is clicked', async () => {
    const user = userEvent.setup();
    const onDismiss = vi.fn();

    render(
      <ErrorSheet
        title="Save failed"
        description="Please try again"
        onDismiss={onDismiss}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Dismiss' }));

    expect(onDismiss).toHaveBeenCalledTimes(1);
  });
});
