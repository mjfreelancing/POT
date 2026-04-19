import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, test, vi } from 'vitest';

import EnrichedCalendar from '@/components/picker/EnrichedCalendar';

function getSelectedDayButton(): HTMLButtonElement {
  const selectedDayButton = document.querySelector(
    'button[aria-selected="true"]',
  );

  if (!(selectedDayButton instanceof HTMLButtonElement)) {
    throw new Error('Expected exactly one selected day button.');
  }

  return selectedDayButton;
}

function querySelectedDayButton(): HTMLButtonElement | null {
  const selectedDayButton = document.querySelector(
    'button[aria-selected="true"]',
  );

  return selectedDayButton instanceof HTMLButtonElement
    ? selectedDayButton
    : null;
}

describe('EnrichedCalendar', () => {
  test('keeps selected 31 visible across Mar -> Apr -> May navigation', () => {
    const onDateAccepted = vi.fn();
    const onDateChange = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date(2026, 2, 31)}
        onDateAccepted={onDateAccepted}
        onDateChange={onDateChange}
      />,
    );

    expect(screen.getByText('Mar 2026')).toBeInTheDocument();

    const selectedInMarch = getSelectedDayButton();
    expect(selectedInMarch).toHaveTextContent('31');
    expect(selectedInMarch.className).not.toContain('day-outside');

    fireEvent.click(screen.getByTitle('Next Month'));

    expect(screen.getByText('Apr 2026')).toBeInTheDocument();

    const selectedInApril = getSelectedDayButton();
    expect(selectedInApril).toHaveTextContent('31');
    expect(selectedInApril.className).toContain('day-outside');

    fireEvent.click(screen.getByTitle('Next Month'));

    expect(screen.getByText('May 2026')).toBeInTheDocument();

    const selectedInMay = getSelectedDayButton();
    expect(selectedInMay).toHaveTextContent('31');
    expect(selectedInMay.className).not.toContain('day-outside');

    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    const expectedMay31 = new Date(2026, 4, 31);

    expect(onDateChange).toHaveBeenCalledWith(expectedMay31);
    expect(onDateAccepted).toHaveBeenCalledWith(expectedMay31);
  });

  test('hides selected day in Feb and restores it in Jan when navigating backward', () => {
    const onDateAccepted = vi.fn();
    const onDateChange = vi.fn();

    render(
      <EnrichedCalendar
        selectedDate={new Date(2026, 2, 31)}
        onDateAccepted={onDateAccepted}
        onDateChange={onDateChange}
      />,
    );

    expect(screen.getByText('Mar 2026')).toBeInTheDocument();

    const selectedInMarch = getSelectedDayButton();
    expect(selectedInMarch).toHaveTextContent('31');
    expect(selectedInMarch.className).not.toContain('day-outside');

    fireEvent.click(screen.getByTitle('Previous Month'));

    expect(screen.getByText('Feb 2026')).toBeInTheDocument();
    expect(querySelectedDayButton()).toBeNull();

    fireEvent.click(screen.getByTitle('Previous Month'));

    expect(screen.getByText('Jan 2026')).toBeInTheDocument();

    const selectedInJanuary = getSelectedDayButton();
    expect(selectedInJanuary).toHaveTextContent('31');
    expect(selectedInJanuary.className).not.toContain('day-outside');

    fireEvent.click(screen.getByRole('button', { name: 'Accept' }));

    const expectedJanuary31 = new Date(2026, 0, 31);

    expect(onDateChange).toHaveBeenCalledWith(expectedJanuary31);
    expect(onDateAccepted).toHaveBeenCalledWith(expectedJanuary31);
  });
});
