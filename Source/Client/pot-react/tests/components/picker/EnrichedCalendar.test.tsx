import { fireEvent, render, screen } from '@testing-library/react';
import React from 'react';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import EnrichedCalendar from '@/components/picker/EnrichedCalendar';

const calendarSpy = vi.fn();

vi.mock('@/components/ui/button', () => ({
  Button: ({
    children,
    onClick,
    disabled,
    title,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    disabled?: boolean;
    title?: string;
    size?: string;
    variant?: string;
    className?: string;
  }) => (
    <button type="button" onClick={onClick} disabled={disabled} title={title}>
      {children}
    </button>
  ),
}));

vi.mock('@/components/ui/calendar', () => ({
  Calendar: ({
    selected,
    month,
    fromDate,
    toDate,
    disabled,
    onSelect,
    onMonthChange,
  }: {
    selected: Date | undefined;
    month: Date;
    fromDate?: Date;
    toDate?: Date;
    disabled?: (date: Date) => boolean;
    onSelect?: (value: Date | undefined, dayClicked: Date) => void;
    onMonthChange?: (month: Date) => void;
    mode?: string;
  }) => {
    calendarSpy({ selected, month, fromDate, toDate, disabled });

    return (
      <div data-testid="calendar">
        <button
          type="button"
          onClick={() => {
            onSelect?.(
              new Date('2026-03-15T00:00:00.000Z'),
              new Date('2026-03-15T00:00:00.000Z'),
            );
          }}
        >
          Select date
        </button>
        <button
          type="button"
          onClick={() => {
            onSelect?.(
              undefined,
              selected ?? new Date('2026-03-15T00:00:00.000Z'),
            );
          }}
        >
          Emit undefined selected date
        </button>
        <button
          type="button"
          onClick={() => {
            onSelect?.(
              new Date('1800-01-01T00:00:00.000Z'),
              new Date('1800-01-01T00:00:00.000Z'),
            );
          }}
        >
          Select out of range date
        </button>
        <button
          type="button"
          onClick={() => {
            onMonthChange?.(new Date('2026-04-01T00:00:00.000Z'));
          }}
        >
          Change month
        </button>
      </div>
    );
  },
}));

describe('EnrichedCalendar', () => {
  beforeEach(() => {
    calendarSpy.mockClear();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  test('calls onDateAccepted with selected date when accept is clicked', () => {
    const onDateAccepted = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date('2026-01-10T00:00:00.000Z')}
        onDateAccepted={onDateAccepted}
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Select date' }));
    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    expect(onDateAccepted).toHaveBeenCalledWith(
      new Date('2026-03-15T00:00:00.000Z'),
    );
  });

  test('selects today when enabled and accepts that date', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-10T12:00:00.000Z'));

    const onDateChange = vi.fn();
    const onDateAccepted = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={undefined}
        onDateAccepted={onDateAccepted}
        onDateChange={onDateChange}
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Today' }));
    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    const expectedToday = new Date(2026, 4, 10);

    expect(onDateChange).toHaveBeenCalledWith(expectedToday);

    expect(onDateAccepted).toHaveBeenCalledWith(expectedToday);
  });

  test('calls onCancel when cancel button is clicked', () => {
    const onCancel = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={undefined}
        onDateAccepted={vi.fn()}
        onCancel={onCancel}
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  test('forwards date constraints and disables Today when today is outside range', () => {
    const minDate = new Date('2100-01-01T00:00:00.000Z');
    const maxDate = new Date('2100-12-31T00:00:00.000Z');

    render(
      <EnrichedCalendar
        selectedDate={undefined}
        onDateAccepted={vi.fn()}
        minDate={minDate}
        maxDate={maxDate}
      />,
    );

    const lastCalendarCall = calendarSpy.mock.calls[
      calendarSpy.mock.calls.length - 1
    ]?.[0] as {
      fromDate: Date;
      toDate: Date;
    };

    expect(lastCalendarCall.fromDate).toEqual(minDate);
    expect(lastCalendarCall.toDate).toEqual(maxDate);
    expect(screen.getByRole('button', { name: 'Today' })).toBeDisabled();
  });

  test('ignores out-of-range date selections', () => {
    const onDateAccepted = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date('2026-01-10T00:00:00.000Z')}
        onDateAccepted={onDateAccepted}
        minDate={new Date('2026-01-01T00:00:00.000Z')}
        maxDate={new Date('2026-12-31T00:00:00.000Z')}
      />,
    );

    fireEvent.click(
      screen.getByRole('button', { name: 'Select out of range date' }),
    );
    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    expect(onDateAccepted).toHaveBeenCalledWith(
      new Date('2026-01-10T00:00:00.000Z'),
    );
  });

  test('does not clear selected date when calendar emits undefined for selected day', () => {
    const onDateAccepted = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date('2026-01-10T00:00:00.000Z')}
        onDateAccepted={onDateAccepted}
      />,
    );

    fireEvent.click(
      screen.getByRole('button', { name: 'Emit undefined selected date' }),
    );
    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    expect(onDateAccepted).toHaveBeenCalledWith(
      new Date('2026-01-10T00:00:00.000Z'),
    );
  });

  test('moves across months and years while respecting min and max boundaries', () => {
    const onYearChange = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date('2026-03-31T00:00:00.000Z')}
        onDateAccepted={vi.fn()}
        minDate={new Date('2026-01-01T00:00:00.000Z')}
        maxDate={new Date('2026-12-31T00:00:00.000Z')}
        onYearChange={onYearChange}
      />,
    );

    expect(screen.getByText('Mar 2026')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Next Month' }));
    expect(screen.getByText('Apr 2026')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Previous Month' }));
    expect(screen.getByText('Mar 2026')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Previous Year' }));
    // Clamped by minDate year/month.
    expect(screen.getByText('Jan 2026')).toBeInTheDocument();
    expect(onYearChange).toHaveBeenCalledWith(2026);

    fireEvent.click(screen.getByRole('button', { name: 'Next Year' }));
    // Clamped by maxDate year/month.
    expect(screen.getByText('Dec 2026')).toBeInTheDocument();
    expect(onYearChange).toHaveBeenCalledWith(2026);
  });

  test('preserves original selected date when navigating to month with fewer days', () => {
    const onDateAccepted = vi.fn();
    const onDateChange = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date('2026-03-31T00:00:00.000Z')}
        onDateAccepted={onDateAccepted}
        onDateChange={onDateChange}
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Next Month' }));
    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    expect(screen.getByText('Apr 2026')).toBeInTheDocument();
    // No valid equivalent day in April, so no date-change callback fires.
    expect(onDateChange).not.toHaveBeenCalled();
    // Selected date remains the last valid picked date.
    expect(onDateAccepted).toHaveBeenCalledWith(
      new Date('2026-03-31T00:00:00.000Z'),
    );
  });

  test('calls onMonthChange callback when calendar month changes', () => {
    const onMonthChange = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={undefined}
        onDateAccepted={vi.fn()}
        onMonthChange={onMonthChange}
      />,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Change month' }));

    expect(onMonthChange).toHaveBeenCalledWith(
      new Date('2026-04-01T00:00:00.000Z'),
    );
  });
});
