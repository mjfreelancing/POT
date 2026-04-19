import { fireEvent, render, screen } from '@testing-library/react';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';

vi.mock('@/components/ui/popover', () => ({
  Popover: ({
    children,
    open,
  }: {
    children: React.ReactNode;
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
  }) => (
    <div data-testid="popover" data-open={String(open)}>
      {children}
    </div>
  ),
  PopoverTrigger: ({
    children,
  }: {
    children: React.ReactNode;
    asChild?: boolean;
  }) => <>{children}</>,
  PopoverContent: ({
    children,
    align,
  }: {
    children: React.ReactNode;
    align?: string;
    className?: string;
  }) => (
    <div data-testid="popover-content" data-align={align}>
      {children}
    </div>
  ),
}));

describe('EnrichedDatePicker', () => {
  test('renders default trigger label when no date is selected', () => {
    render(
      <EnrichedDatePicker selectedDate={undefined} onDateAccepted={vi.fn()} />,
    );

    expect(
      screen.getByRole('button', { name: /pick a date/i }),
    ).toBeInTheDocument();
  });

  test('renders custom trigger label when provided', () => {
    render(
      <EnrichedDatePicker
        selectedDate={new Date('2026-01-01T00:00:00.000Z')}
        onDateAccepted={vi.fn()}
        triggerLabel={() => 'Custom date label'}
      />,
    );

    expect(
      screen.getByRole('button', { name: /custom date label/i }),
    ).toBeInTheDocument();
  });

  test('forwards constrained calendar props and align option', () => {
    const minDate = new Date('2100-01-01T00:00:00.000Z');
    const maxDate = new Date('2100-12-31T00:00:00.000Z');

    render(
      <EnrichedDatePicker
        selectedDate={new Date('2026-06-15T00:00:00.000Z')}
        onDateAccepted={vi.fn()}
        minDate={minDate}
        maxDate={maxDate}
        popoverContentAlign="end"
      />,
    );

    expect(screen.getByTestId('popover-content')).toHaveAttribute(
      'data-align',
      'end',
    );

    expect(screen.getByText('Jan 2100')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Today' })).toBeDisabled();
  });

  test('handles accept and cancel callbacks from calendar', () => {
    const onDateAccepted = vi.fn();
    const onCancel = vi.fn();
    const selectedDate = new Date(2026, 0, 10);

    render(
      <EnrichedDatePicker
        selectedDate={selectedDate}
        onDateAccepted={onDateAccepted}
        onCancel={onCancel}
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(onDateAccepted).toHaveBeenCalledWith(selectedDate);
    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
