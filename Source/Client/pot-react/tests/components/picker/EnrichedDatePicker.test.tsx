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

vi.mock('@/components/ui/button', () => ({
  Button: ({
    children,
    onClick,
    disabled,
    id,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    disabled?: boolean;
    id?: string;
    className?: string;
    variant?: string;
  }) => (
    <button type="button" onClick={onClick} disabled={disabled} id={id}>
      {children}
    </button>
  ),
}));

vi.mock('@/components/picker/EnrichedCalendar', () => ({
  default: ({
    selectedDate,
    onDateAccepted,
    onCancel,
    showBorder,
    compactHeader,
    minDate,
    maxDate,
  }: {
    selectedDate: Date | undefined;
    onDateAccepted: (date: Date | undefined) => void;
    onCancel: () => void;
    showBorder?: boolean;
    compactHeader?: boolean;
    minDate?: Date;
    maxDate?: Date;
  }) => (
    <div
      data-testid="enriched-calendar"
      data-selected={selectedDate?.toISOString() ?? ''}
      data-show-border={String(showBorder)}
      data-compact-header={String(compactHeader)}
      data-min-date={minDate?.toISOString() ?? ''}
      data-max-date={maxDate?.toISOString() ?? ''}
    >
      <button
        type="button"
        onClick={() => {
          onDateAccepted(new Date('2026-02-01T00:00:00.000Z'));
        }}
      >
        Accept from calendar
      </button>
      <button type="button" onClick={onCancel}>
        Cancel from calendar
      </button>
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
    const minDate = new Date('2026-01-01T00:00:00.000Z');
    const maxDate = new Date('2026-12-31T00:00:00.000Z');

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

    const calendar = screen.getByTestId('enriched-calendar');

    expect(calendar).toHaveAttribute('data-show-border', 'false');
    expect(calendar).toHaveAttribute('data-compact-header', 'false');
    expect(calendar).toHaveAttribute('data-min-date', minDate.toISOString());
    expect(calendar).toHaveAttribute('data-max-date', maxDate.toISOString());
  });

  test('handles accept and cancel callbacks from calendar', () => {
    const onDateAccepted = vi.fn();
    const onCancel = vi.fn();

    render(
      <EnrichedDatePicker
        selectedDate={undefined}
        onDateAccepted={onDateAccepted}
        onCancel={onCancel}
      />,
    );

    fireEvent.click(
      screen.getByRole('button', { name: 'Accept from calendar' }),
    );
    fireEvent.click(
      screen.getByRole('button', { name: 'Cancel from calendar' }),
    );

    expect(onDateAccepted).toHaveBeenCalledWith(
      new Date('2026-02-01T00:00:00.000Z'),
    );
    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
