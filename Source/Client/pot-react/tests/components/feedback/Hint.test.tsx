import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test } from 'vitest';

import Hint from '@/components/feedback/Hint';

describe('Hint', () => {
  test('renders children untouched when no hint is provided', () => {
    render(
      <Hint>
        <span>Amount</span>
      </Hint>,
    );

    expect(screen.getByText('Amount')).toBeInTheDocument();
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  test('opens the tooltip on hover and describes the trigger while open', async () => {
    const user = userEvent.setup();

    render(
      <Hint hint="The committed amount">
        <button type="button">Amount</button>
      </Hint>,
    );

    const trigger = screen.getByRole('button', { name: 'Amount' });

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await user.hover(trigger);

    const tooltip = await screen.findByRole('tooltip');

    expect(tooltip).toHaveTextContent('The committed amount');
    expect(trigger).toHaveAttribute('aria-describedby', tooltip.id);
  });

  test('sizes and wraps the tooltip content for long hints', async () => {
    const user = userEvent.setup();

    render(
      <Hint hint="A deliberately long hint that should wrap within the tooltip's maximum width">
        <button type="button">Amount</button>
      </Hint>,
    );

    await user.hover(screen.getByRole('button', { name: 'Amount' }));

    const tooltip = await screen.findByRole('tooltip');

    expect(tooltip).toHaveClass('max-w-sm', 'text-wrap');
  });
});
