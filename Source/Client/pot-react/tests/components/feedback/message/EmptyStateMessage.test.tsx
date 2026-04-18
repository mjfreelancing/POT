import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import EmptyStateMessage from '@/components/feedback/message/EmptyStateMessage';

describe('EmptyStateMessage', () => {
  test('renders title and description', () => {
    render(
      <EmptyStateMessage
        title="Create an account to get started"
        description="Every income entry must be assigned to an account."
      />,
    );

    expect(
      screen.getByText('Create an account to get started'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Every income entry must be assigned to an account.'),
    ).toBeInTheDocument();
  });

  test('does not render action buttons when actions are not provided', () => {
    render(
      <EmptyStateMessage
        title="Create an account to get started"
        description="Every expense entry must be assigned to an account."
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Create Account' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Go to Accounts' }),
    ).not.toBeInTheDocument();
  });

  test('renders and triggers primary action', async () => {
    const user = userEvent.setup();
    const onPrimaryAction = vi.fn();

    render(
      <EmptyStateMessage
        title="Create an account to get started"
        description="Every income entry must be assigned to an account."
        primaryActionLabel="Create Account"
        onPrimaryAction={onPrimaryAction}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Create Account' }));

    expect(onPrimaryAction).toHaveBeenCalledTimes(1);
    expect(
      screen.queryByRole('button', { name: 'Go to Accounts' }),
    ).not.toBeInTheDocument();
  });

  test('renders and triggers both primary and secondary actions', async () => {
    const user = userEvent.setup();
    const onPrimaryAction = vi.fn();
    const onSecondaryAction = vi.fn();

    render(
      <EmptyStateMessage
        title="Create an account to get started"
        description="Every expense entry must be assigned to an account."
        primaryActionLabel="Create Account"
        onPrimaryAction={onPrimaryAction}
        secondaryActionLabel="Go to Accounts"
        onSecondaryAction={onSecondaryAction}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Create Account' }));
    await user.click(screen.getByRole('button', { name: 'Go to Accounts' }));

    expect(onPrimaryAction).toHaveBeenCalledTimes(1);
    expect(onSecondaryAction).toHaveBeenCalledTimes(1);
  });
});
