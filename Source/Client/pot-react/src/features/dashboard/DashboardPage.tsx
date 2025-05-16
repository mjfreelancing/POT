import { ColumnDef } from '@tanstack/react-table';
import { useEffect, useState } from 'react';
import { format } from 'date-fns';
import { CalendarIcon } from 'lucide-react';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { EnrichedCalendar } from '@/components/picker/EnrichedCalendar'; // Renamed import
import {
  createMoneyValueColumn,
  DataTable,
} from '@/components/table/DataTable';
import { Account } from '@/data/accounts/account';
import { DisplayError } from '@/lib/errors/displayError';
import { Button } from '@/components/ui/button';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { cn } from '@/lib/utils';
import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';

// Temporary - a WIP

const columns: ColumnDef<Account>[] = [
  {
    accessorKey: 'bsb_number',
    header: 'BSB / Number',
    cell: ({ row }) => {
      const { bsb, number } = row.original;
      return `(${bsb}) ${number}`;
    },
  },
  {
    accessorKey: 'description',
    header: 'Description',
  },
  createMoneyValueColumn<Account>('balance', 'Balance'),
  createMoneyValueColumn<Account>('available', 'Available'),
];

const DashboardPage = () => {
  const [error, setError] = useState<DisplayError | null>(null);
  const { data: result, isLoading } = useApiGetAllAccounts();
  const [isPopoverOpen, setIsPopoverOpen] = useState(false);

  useEffect(() => {
    // Transient error handling
    if (result) {
      if (result.success) {
        // Reset error state, such as after a network loss
        setError(null);
      } else {
        setError({
          title: result.error.code,
          description: result.error.description,
        });
      }
    }
  }, [result]);

  const accounts = result?.success ? result.value : [];

  // *********************

  const [selectedDate, setSelectedDate] = useState<Date | undefined>(
    new Date(),
  );

  const handleDateAccepted = (acceptedDate: Date | undefined) => {
    setSelectedDate(acceptedDate);
    setIsPopoverOpen(false); // Close popover on accept, if open
    console.log('Date selection accepted');
  };

  const handleCalendarCancel = () => {
    setIsPopoverOpen(false); // Close popover on cancel, if open
    console.log('Date selection cancelled');
  };

  // *********************

  return (
    <>
      <div className="container mx-auto py-4 px-4 flex flex-col gap-4">
        <div className="w-full">
          {/* Wrapper to ensure DataTable takes full width */}
          <DataTable columns={columns} data={accounts} />
        </div>
        {/* New wrapper for all picker components, aligned left */}
        <div className="flex flex-row items-start gap-4">
          <EnrichedCalendar
            showBorder={false}
            compactHeader={true}
            date={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedCalendar
            date={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedCalendar
            showBorder={false}
            compactHeader={false}
            date={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedDatePicker
            selectedDate={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
        </div>
        <LoadingMessage isLoading={isLoading} />
      </div>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
};

export default DashboardPage;
