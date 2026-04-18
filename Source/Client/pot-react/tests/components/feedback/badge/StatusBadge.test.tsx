import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import StatusBadge from '@/components/feedback/badge/StatusBadge';

vi.mock('@/components/ui/tooltip', () => ({
  TooltipProvider: ({ children }: { children: React.ReactNode }) => (
    <>{children}</>
  ),
  Tooltip: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  TooltipTrigger: ({ children }: { children: React.ReactNode }) => (
    <>{children}</>
  ),
  TooltipContent: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
}));

describe('StatusBadge', () => {
  test('renders children with default gray style', () => {
    render(<StatusBadge>Pending</StatusBadge>);

    const badge = screen.getByText('Pending');

    expect(badge).toHaveClass('bg-gray-50', 'text-gray-700', 'border-gray-200');
  });

  test('renders tooltip content when tooltip is provided', () => {
    render(<StatusBadge tooltip="More details">Pending</StatusBadge>);

    expect(screen.getByText('More details')).toBeInTheDocument();
  });

  test('calls onClick when badge is interactive', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();

    render(<StatusBadge onClick={onClick}>Pending</StatusBadge>);

    await user.click(screen.getByText('Pending'));

    expect(onClick).toHaveBeenCalledTimes(1);
  });
});
