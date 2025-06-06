import { format } from 'date-fns';
import { CalendarIcon } from 'lucide-react';
import type { ComponentProps } from 'react';
import * as React from 'react';

import { Button } from '@/components/ui/button';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { cn } from '@/lib/utils';

import EnrichedCalendar from './EnrichedCalendar';

// Props for EnrichedCalendar, excluding those managed by EnrichedDatePicker
type EnrichedCalendarPassthroughProps = Omit<
  ComponentProps<typeof EnrichedCalendar>,
  'selectedDate' | 'onDateAccepted' | 'onCancel'
>;

export type EnrichedDatePickerProps = EnrichedCalendarPassthroughProps & {
  selectedDate: Date | undefined;
  onDateAccepted: (date: Date | undefined) => void;
  onCancel?: () => void;
  triggerClassName?: string;
  triggerLabel?: (date: Date | undefined) => React.ReactNode;
  popoverContentAlign?: ComponentProps<typeof PopoverContent>['align'];
};

function EnrichedDatePicker({
  selectedDate,
  onDateAccepted,
  onCancel,
  triggerClassName,
  triggerLabel,
  popoverContentAlign = 'start',
  ...calendarProps
}: EnrichedDatePickerProps) {
  const [isPopoverOpen, setIsPopoverOpen] = React.useState(false);

  const handleDateAccept = (acceptedDate: Date | undefined) => {
    onDateAccepted(acceptedDate);
    setIsPopoverOpen(false);
  };

  const handleDateCancel = () => {
    setIsPopoverOpen(false);

    if (onCancel) {
      onCancel();
    }
  };

  const defaultTriggerLabel = (date: Date | undefined) =>
    date ? format(date, 'PPP') : <span>Pick a date</span>;

  return (
    <Popover open={isPopoverOpen} onOpenChange={setIsPopoverOpen}>
      <PopoverTrigger asChild>
        <Button
          variant={'outline'}
          className={cn(
            'w-[240px] justify-start text-left font-normal',
            !selectedDate && 'text-muted-foreground',
            triggerClassName,
          )}
        >
          <CalendarIcon className="mr-2 size-4" />
          {triggerLabel
            ? triggerLabel(selectedDate)
            : defaultTriggerLabel(selectedDate)}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align={popoverContentAlign}>
        <EnrichedCalendar
          {...calendarProps}
          selectedDate={selectedDate}
          onDateAccepted={handleDateAccept}
          onCancel={handleDateCancel}
          showBorder={false} // Will thicken the border if enabled
          compactHeader={false}
        />
      </PopoverContent>
    </Popover>
  );
}

export { EnrichedDatePicker };
