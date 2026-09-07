import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import { Calendar } from '@/components/ui/calendar';

// Renders the real react-day-picker through the shadcn `ui/calendar` wrapper.
// Guards the day-grid structure + navigation that react-day-picker 8 -> 10
// touches (classNames keys, day markup, nav labels).

const localDate = (year: number, month: number, day: number): Date =>
  new Date(year, month, day);

describe('Calendar (ui/calendar)', () => {
  test('renders a single-mode month with selectable day buttons', async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();

    render(
      <Calendar
        mode="single"
        selected={localDate(2026, 3, 5)}
        onSelect={onSelect}
      />,
    );

    const dayButtons = screen.getAllByRole('gridcell');
    expect(dayButtons.length).toBeGreaterThan(28);

    const dayCell = screen.getByRole('gridcell', { name: '10' });
    const dayButton = dayCell.querySelector('button');
    expect(dayButton).not.toBeNull();

    await user.click(dayButton as HTMLButtonElement);
    expect(onSelect).toHaveBeenCalled();
    expect(onSelect.mock.calls[0][0]).toBeInstanceOf(Date);
  });

  test('renders a range-mode month without throwing', () => {
    render(
      <Calendar
        mode="range"
        selected={{
          from: localDate(2026, 3, 3),
          to: localDate(2026, 3, 10),
        }}
      />,
    );

    expect(screen.getAllByRole('gridcell').length).toBeGreaterThan(28);
  });
});
