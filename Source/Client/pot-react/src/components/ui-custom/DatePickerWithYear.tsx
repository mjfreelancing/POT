import * as React from 'react';
import { format } from 'date-fns';
import { CalendarIcon, ChevronLeft, ChevronRight } from 'lucide-react'; // Import Chevron icons

import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';

export function DatePickerWithYear() {
  const [date, setDate] = React.useState<Date>();
  const [currentYear, setCurrentYear] = React.useState(
    new Date().getFullYear(),
  );
  const [currentMonth, setCurrentMonth] = React.useState(new Date().getMonth());

  const handleYearChange = (offset: number) => {
    setCurrentYear(prevYear => prevYear + offset);
  };

  const handleDateSelect = (selectedDate: Date | undefined) => {
    setDate(selectedDate);
    if (selectedDate) {
      setCurrentYear(selectedDate.getFullYear());
      setCurrentMonth(selectedDate.getMonth());
    }
  };

  const handleMonthChange = (newMonth: Date) => {
    setCurrentYear(newMonth.getFullYear());
    setCurrentMonth(newMonth.getMonth());
  };

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant={'outline'}
          className={cn(
            'w-[240px] justify-start text-left font-normal',
            !date && 'text-muted-foreground',
          )}
        >
          <CalendarIcon />
          {date ? format(date, 'PPP') : <span>Pick a date</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <div className="p-2">
          <div className="flex items-center justify-between mb-2 px-4">
            <Button
              variant="outline"
              size="icon"
              className={cn(
                'size-7 bg-transparent p-0 opacity-50 hover:opacity-100 border rounded-md flex items-center justify-center',
              )}
              onClick={() => handleYearChange(-1)}
            >
              <ChevronLeft className="size-4" />
            </Button>
            <span className="text-sm font-medium">{currentYear}</span>
            <Button
              variant="outline"
              size="icon"
              className={cn(
                'size-7 bg-transparent p-0 opacity-50 hover:opacity-100 border rounded-md flex items-center justify-center',
              )}
              onClick={() => handleYearChange(1)}
            >
              <ChevronRight className="size-4" />
            </Button>
          </div>
          <hr className="my-2 border-muted mx-4" />
          <Calendar
            key={`${currentYear}-${currentMonth}`} // Force re-render when year or month changes
            mode="single"
            selected={date}
            onSelect={handleDateSelect}
            onMonthChange={handleMonthChange}
            initialFocus
            defaultMonth={new Date(currentYear, currentMonth, 1)}
          />
        </div>
      </PopoverContent>
    </Popover>
  );
}
