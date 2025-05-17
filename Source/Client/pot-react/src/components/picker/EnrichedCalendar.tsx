import { format, addMonths, subMonths, addYears, subYears } from 'date-fns';
import {
  ChevronsLeft,
  ChevronsRight,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import * as React from 'react';
import type { DayPickerSingleProps } from 'react-day-picker';

import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { cn } from '@/lib/utils';

type EnrichedCalendarProps = Omit<
  React.ComponentProps<typeof Calendar>,
  'onSelect' | 'selected' | 'month' | 'components' | 'mode'
> & {
  selectedDate: Date | undefined;
  onDateAccepted: (acceptedDate: Date | undefined) => void;
  onCancel?: () => void;
  onDateChange?: (newDate: Date | undefined) => void;
  onMonthChange?: (month: Date) => void;
  onYearChange?: (year: number) => void;
  showBorder?: boolean;
  compactHeader?: boolean;
  caption?: React.ReactNode;
};

/**
 * Renders an enriched calendar component with custom navigation and action controls.
 *
 * This component provides a calendar UI for selecting a single date, with additional controls
 * for navigating months and years, jumping to today, and accepting or cancelling the selection.
 * It is designed to be used as a date picker with enhanced UX features.
 *
 * @param {EnrichedCalendarProps} props - The props for the EnrichedCalendar component.
 * @param {Date | undefined} props.selectedDate - The currently selected date (controlled).
 * @param {(date: Date | undefined) => void} [props.onDateChange] - Callback fired when the date selection changes.
 * @param {(date: Date | undefined) => void} props.onDateAccepted - Callback fired when the user accepts the selected date.
 * @param {() => void} [props.onCancel] - Callback fired when the user cancels the selection.
 * @param {(month: Date) => void} [props.onMonthChange] - Callback fired when the displayed month changes.
 * @param {(year: number) => void} [props.onYearChange] - Callback fired when the displayed year changes.
 * @param {boolean} [props.showBorder=true] - Whether to display a border around the calendar.
 * @param {boolean} [props.compactHeader=false] - Whether to use compact padding for the header.
 * @param {React.ReactNode} [props.caption] - Optional React node (e.g., JSX element) to display as a caption above the navigation header.
 */
function EnrichedCalendar({
  selectedDate: propDate,
  onDateChange,
  onDateAccepted,
  onCancel,
  onMonthChange: parentOnMonthChange,
  onYearChange,
  showBorder = true,
  compactHeader = false,
  caption,
  ...calendarProps
}: EnrichedCalendarProps): JSX.Element {
  const [pickerDate, setPickerDate] = React.useState<Date | undefined>(
    propDate,
  );
  const [currentDisplayMonth, setCurrentDisplayMonth] = React.useState<Date>(
    propDate || new Date(),
  );

  React.useEffect(() => {
    setPickerDate(propDate);

    if (propDate) {
      setCurrentDisplayMonth(
        new Date(propDate.getFullYear(), propDate.getMonth(), 1),
      );
    } else {
      setCurrentDisplayMonth(
        new Date(new Date().getFullYear(), new Date().getMonth(), 1),
      );
    }
  }, [propDate]);

  const handleDateSelectInCalendar: DayPickerSingleProps['onSelect'] = (
    newSelection,
    dayClicked,
  ) => {
    if (
      newSelection === undefined &&
      pickerDate &&
      dayClicked.getTime() === pickerDate.getTime()
    ) {
      return;
    }

    setPickerDate(newSelection);

    if (onDateChange) {
      onDateChange(newSelection);
    }
  };

  const handlePrevMonth = () => {
    setCurrentDisplayMonth(prev => subMonths(prev, 1));
  };

  const handleNextMonth = () => {
    setCurrentDisplayMonth(prev => addMonths(prev, 1));
  };

  const handlePrevYear = () => {
    setCurrentDisplayMonth(prev => {
      const newMonth = subYears(prev, 1);

      if (onYearChange) {
        onYearChange(newMonth.getFullYear());
      }

      return newMonth;
    });
  };

  const handleNextYear = () => {
    setCurrentDisplayMonth(prev => {
      const newMonth = addYears(prev, 1);

      if (onYearChange) {
        onYearChange(newMonth.getFullYear());
      }

      return newMonth;
    });
  };

  const handleToday = () => {
    const today = new Date();
    setPickerDate(today);

    if (onDateChange) {
      onDateChange(today);
    }

    setCurrentDisplayMonth(new Date(today.getFullYear(), today.getMonth(), 1));
  };

  const handleAcceptInternal = () => {
    onDateAccepted(pickerDate);
  };

  const handleCancelInternal = () => {
    if (onCancel) {
      onCancel();
    }
  };

  const handleInternalMonthChange = (newMonth: Date) => {
    setCurrentDisplayMonth(newMonth);

    if (parentOnMonthChange) {
      parentOnMonthChange(newMonth);
    }
  };

  return (
    /**
     * The outermost `<div className="flex flex-col">` is required to provide a flex container for
     * the calendar layout, ensuring that the custom navigation header, calendar grid, and actions
     * footer are stacked vertically and styled consistently.
     */
    <div className={cn('flex flex-col', showBorder && 'border rounded-md')}>
      {/* Optional Caption Element */}
      {caption && (
        <div
          className={cn(
            'flex justify-center w-full text-sm font-medium px-2 pb-2 border-b',
            compactHeader ? 'pt-0' : 'pt-2',
          )}
        >
          {caption}
        </div>
      )}

      {/* Custom Navigation Header */}
      <div
        className={cn(
          'flex items-center justify-between px-2',
          compactHeader ? 'pt-0' : 'pt-2', // Top padding
          caption ? 'pb-2' : 'pb-2 border-b', // Conditional bottom padding and border based on caption presence
        )}
      >
        <div className="flex items-center gap-1">
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={handlePrevYear}
            title="Previous Year"
          >
            {/* The ChevronsLeft/ChevronsRight are 2 pixels smaller than ChevronLeft/ChevronRight
             so setting a width and height to make them slightly bigger */}
            <ChevronsLeft style={{ width: 22, height: 22 }} />
          </Button>
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={handlePrevMonth}
            title="Previous Month"
          >
            <ChevronLeft />
          </Button>
        </div>
        <div className="text-sm font-medium">
          {format(currentDisplayMonth, 'MMM yyyy')}
        </div>
        <div className="flex items-center gap-1">
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={handleNextMonth}
            title="Next Month"
          >
            <ChevronRight />
          </Button>
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8"
            onClick={handleNextYear}
            title="Next Year"
          >
            {/* The ChevronsLeft/ChevronsRight are 2 pixels smaller than ChevronLeft/ChevronRight
             so setting a width and height to make them slightly bigger */}
            <ChevronsRight style={{ width: 22, height: 22 }} />
          </Button>
        </div>
      </div>

      <Calendar
        mode="single"
        selected={pickerDate}
        onSelect={handleDateSelectInCalendar}
        month={currentDisplayMonth}
        onMonthChange={handleInternalMonthChange}
        initialFocus
        showOutsideDays
        components={{
          Caption: () => null,
        }}
        {...calendarProps}
      />
      {/* Actions Footer */}
      <div className="flex justify-between items-center px-3 pt-2 pb-3 border-t">
        <Button variant="outline" size="sm" onClick={handleToday}>
          Today
        </Button>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleCancelInternal}>
            Cancel
          </Button>
          <Button size="sm" onClick={handleAcceptInternal}>
            Accept
          </Button>
        </div>
      </div>
    </div>
  );
}

export { EnrichedCalendar };
