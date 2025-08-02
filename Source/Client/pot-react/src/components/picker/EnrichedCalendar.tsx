import { addMonths, addYears, format, subMonths, subYears } from 'date-fns';
import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from 'lucide-react';
import * as React from 'react';
import type { DayPickerSingleProps } from 'react-day-picker';

import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { cn, isAfterDate, isBeforeDate, isSameDate, localToday } from '@/lib';

// Helper functions
/**
 * Returns a date set to the first day of the specified month and year.
 */
function getFirstDayOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

type EnrichedCalendarProps = Omit<
  React.ComponentProps<typeof Calendar>,
  'onSelect' | 'selected' | 'month' | 'components' | 'mode'
> & {
  selectedDate: Date | undefined;
  minDate?: Date | undefined;
  maxDate?: Date | undefined;
  showBorder?: boolean;
  compactHeader?: boolean;
  caption?: React.ReactNode;
  onDateAccepted: (acceptedDate: Date | undefined) => void;
  onCancel?: () => void;
  onDateChange?: (newDate: Date | undefined) => void;
  onMonthChange?: (month: Date) => void;
  onYearChange?: (year: number) => void;
};

/**
 * Renders an enriched calendar component with custom navigation and action controls.
 *
 * This component provides a calendar UI for selecting a single date, with additional controls
 * for navigating months and years, jumping to today, and accepting or cancelling the selection.
 * It is designed to be used as a date picker with enhanced UX features.
 *
 * @param {EnrichedCalendarProps} props - The props for the EnrichedCalendar component.
 * @param {Date | undefined} props.selectedDate - The currently selected date.
 * @param {Date | undefined} props.minDate - The minimum date that can be selected. This expected to be local midnight.
 * @param {Date | undefined} props.maxDate - The maximum date that can be selected. This expected to be local midnight.
 * @param {boolean} [props.showBorder=true] - Whether to display a border around the calendar.
 * @param {boolean} [props.compactHeader=false] - Whether to use compact padding for the header.
 * @param {React.ReactNode} [props.caption] - Optional React node (e.g., JSX element) to display as a caption above the navigation header.
 * @param {(date: Date | undefined) => void} [props.onDateChange] - Callback fired when the date selection changes within the calendar (via clicking days or Today button). Not called on cancel.
 * @param {(date: Date | undefined) => void} props.onDateAccepted - Callback fired when the user explicitly accepts the selected date with the Accept button.
 * @param {() => void} [props.onCancel] - Callback fired when the user cancels the selection. No date change is reported when cancelling.
 * @param {(month: Date) => void} [props.onMonthChange] - Callback fired when the displayed month changes.
 * @param {(year: number) => void} [props.onYearChange] - Callback fired when the displayed year changes.
 */
function EnrichedCalendar({
  selectedDate: propDate,
  minDate = undefined,
  maxDate = undefined,
  showBorder = true,
  compactHeader = false,
  caption,
  onDateChange,
  onDateAccepted,
  onCancel,
  onMonthChange,
  onYearChange,
  ...calendarProps
}: EnrichedCalendarProps): React.JSX.Element {
  const [pickerDate, setPickerDate] = React.useState<Date | undefined>(
    propDate,
  );

  const [currentDisplayMonth, setCurrentDisplayMonth] = React.useState<Date>(
    propDate || localToday(),
  );

  // Only run this effect once on mount or when propDate or min/max dates change
  const isFirstRender = React.useRef(true);

  // Keep track of user selections to prevent them being overridden by props
  const userJustSelected = React.useRef(false);

  React.useEffect(() => {
    // Function to determine the appropriate display month based on a date and constraints
    function getConstrainedDisplayMonth(date: Date | undefined): Date {
      // If no date provided, use today
      const baseDate = date || localToday();

      let displayMonth = getFirstDayOfMonth(baseDate);

      // Apply min constraint if needed
      if (minDate && displayMonth < getFirstDayOfMonth(minDate)) {
        displayMonth = getFirstDayOfMonth(minDate);
      }

      // Apply max constraint if needed
      if (maxDate && displayMonth > getFirstDayOfMonth(maxDate)) {
        displayMonth = getFirstDayOfMonth(maxDate);
      }

      return displayMonth;
    }

    // IMPORTANT: Only update from props during initial render or when we're sure
    // the parent component is explicitly updating the date (not in response to our onChange)
    if (isFirstRender.current) {
      // Initialize picker date from props, but ensure it's within constraints
      let initialDate = propDate;

      // If provided date is outside constraints, use the min/max date instead
      if (initialDate) {
        if (minDate && initialDate < minDate) {
          initialDate = minDate;
        } else if (maxDate && initialDate > maxDate) {
          initialDate = maxDate;
        }
      }

      setPickerDate(initialDate);

      // Calculate appropriate display month respecting constraints
      // Prioritize showing the valid date range if initial date is outside it
      let displayMonth;

      if (minDate && (!initialDate || initialDate < minDate)) {
        // Show min date month if no valid date or date is before min
        displayMonth = getFirstDayOfMonth(minDate);
      } else if (maxDate && initialDate && initialDate > maxDate) {
        // Show max date month if date is after max
        displayMonth = getFirstDayOfMonth(maxDate);
      } else {
        // Otherwise use initial date or today
        displayMonth = getConstrainedDisplayMonth(initialDate);
      }

      setCurrentDisplayMonth(displayMonth);
      isFirstRender.current = false;
    }
    // Only sync with props when propDate explicitly changes from outside
    // AND we didn't just select a date ourselves (which would trigger this effect)
    else if (!userJustSelected.current && propDate !== pickerDate) {
      setPickerDate(propDate);

      // Only update display month if there's a new propDate
      if (propDate) {
        const displayMonth = getConstrainedDisplayMonth(propDate);
        setCurrentDisplayMonth(displayMonth);
      }
    }
    // When userJustSelected is true, we maintain the user's selection
    // instead of syncing with props

    // Reset the user selection flag at the end of the effect
    userJustSelected.current = false;
  }, [propDate, pickerDate, minDate, maxDate]);

  function isBeforeMinDate(date: Date | undefined): boolean {
    if (!minDate || !date) {
      return false;
    }
    return isBeforeDate(date, minDate);
  }

  function isAfterMaxDate(date: Date | undefined): boolean {
    if (!maxDate || !date) {
      return false;
    }
    return isAfterDate(date, maxDate);
  }

  function isTodayDisabled(): boolean {
    const today = localToday();
    return isBeforeMinDate(today) || isAfterMaxDate(today);
  }

  const handleDateSelectInCalendar: DayPickerSingleProps['onSelect'] = (
    newSelection,
    dayClicked,
  ) => {
    // If clicking on the same date as currently selected, maintain current selection
    if (
      newSelection === undefined &&
      pickerDate &&
      isSameDate(dayClicked, pickerDate)
    ) {
      return;
    }

    // Check if date is outside allowed range
    if (isBeforeMinDate(newSelection) || isAfterMaxDate(newSelection)) {
      return;
    }

    // Flag that this is a user selection to prevent it being overridden by props
    userJustSelected.current = true;

    setPickerDate(newSelection);

    // Notify parent of the date change
    if (onDateChange) {
      onDateChange(newSelection);
    }
  };

  function handlePrevMonth() {
    const newMonth = subMonths(currentDisplayMonth, 1);

    if (minDate) {
      const minMonth = getFirstDayOfMonth(minDate);

      if (
        newMonth.getFullYear() < minMonth.getFullYear() ||
        (newMonth.getFullYear() === minMonth.getFullYear() &&
          newMonth.getMonth() < minMonth.getMonth())
      ) {
        return;
      }
    }

    // Only update the display month, not the picker date
    setCurrentDisplayMonth(newMonth);
  }

  function handleNextMonth() {
    const newMonth = addMonths(currentDisplayMonth, 1);

    // Check if we're trying to navigate beyond the month containing maxDate
    if (maxDate) {
      const maxMonth = getFirstDayOfMonth(maxDate);

      if (
        newMonth.getFullYear() > maxMonth.getFullYear() ||
        (newMonth.getFullYear() === maxMonth.getFullYear() &&
          newMonth.getMonth() > maxMonth.getMonth())
      ) {
        return;
      }
    }

    // Only update the display month, not the picker date
    setCurrentDisplayMonth(newMonth);
  }

  function handlePrevYear() {
    const newMonth = subYears(currentDisplayMonth, 1);

    if (minDate) {
      const minMonth = getFirstDayOfMonth(minDate);

      // If trying to navigate before min date's year
      if (newMonth.getFullYear() < minDate.getFullYear()) {
        // Instead of returning, set to min date's month
        setCurrentDisplayMonth(minMonth);

        if (onYearChange) {
          onYearChange(minMonth.getFullYear());
        }

        return;
      }

      // If we're in the same year as minDate, ensure we don't navigate to a month before minDate
      if (
        newMonth.getFullYear() === minDate.getFullYear() &&
        newMonth.getMonth() < minDate.getMonth()
      ) {
        // Set to the month of minDate instead
        newMonth.setMonth(minDate.getMonth());
      }
    }

    // Only update the display month, not the picker date
    setCurrentDisplayMonth(newMonth);

    if (onYearChange) {
      onYearChange(newMonth.getFullYear());
    }
  }

  function handleNextYear() {
    const newMonth = addYears(currentDisplayMonth, 1);

    if (maxDate) {
      const maxMonth = getFirstDayOfMonth(maxDate);

      // If trying to navigate beyond the year containing maxDate
      if (newMonth.getFullYear() > maxDate.getFullYear()) {
        // Instead of returning, set to max date's month
        setCurrentDisplayMonth(maxMonth);

        if (onYearChange) {
          onYearChange(maxMonth.getFullYear());
        }

        return;
      }

      // If we're in the same year as maxDate, ensure we don't navigate to a month after maxDate
      if (
        newMonth.getFullYear() === maxDate.getFullYear() &&
        newMonth.getMonth() > maxDate.getMonth()
      ) {
        // Set to the month of maxDate instead
        newMonth.setMonth(maxDate.getMonth());
      }
    }

    // Only update the display month, not the picker date
    setCurrentDisplayMonth(newMonth);

    if (onYearChange) {
      onYearChange(newMonth.getFullYear());
    }
  }

  function handleToday() {
    const today = localToday();

    // Safety check - the button should be disabled if today is outside the allowed range
    if (isTodayDisabled()) {
      return;
    }

    // Mark this as a user selection to prevent override
    userJustSelected.current = true;

    // Update the picker date to the actual today
    setPickerDate(today);

    // Update the display month to show today's month
    setCurrentDisplayMonth(getFirstDayOfMonth(today));

    // Notify parent of the date change
    if (onDateChange) {
      onDateChange(today);
    }
  }

  function handleAcceptInternal() {
    // Only check constraints if there's a date selected
    if (
      pickerDate &&
      (isBeforeMinDate(pickerDate) || isAfterMaxDate(pickerDate))
    ) {
      return;
    }

    onDateAccepted(pickerDate);
  }

  const handleCancelInternal = () => {
    if (onCancel) {
      onCancel();
    }
  };

  const handleMonthChange = (newMonth: Date) => {
    // Only update the display month, not the picker date
    setCurrentDisplayMonth(newMonth);

    if (onMonthChange) {
      onMonthChange(newMonth);
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
        onMonthChange={handleMonthChange}
        initialFocus
        showOutsideDays
        fromDate={minDate}
        toDate={maxDate}
        components={{
          Caption: () => null,
        }}
        // Prevent selection of days outside allowed range
        disabled={(date: Date) => isBeforeMinDate(date) || isAfterMaxDate(date)}
        {...calendarProps}
      />
      {/* Actions Footer */}
      <div className="flex justify-between items-center px-3 pt-2 pb-3 border-t">
        <Button
          variant="outline"
          size="sm"
          onClick={handleToday}
          disabled={isTodayDisabled()}
        >
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

export default EnrichedCalendar;
export type { EnrichedCalendarProps };
