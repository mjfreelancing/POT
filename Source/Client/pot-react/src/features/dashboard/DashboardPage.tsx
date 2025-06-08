import { ColumnDef } from '@tanstack/react-table';
import { CalendarIcon } from 'lucide-react';
import { useEffect, useState } from 'react';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { EnrichedDatePicker } from '@/components/picker';
import EnrichedCalendar from '@/components/picker/EnrichedCalendar'; // Renamed import
import { createMoneyValueColumn, DataTable } from '@/components/table';
import { Account } from '@/data/account';
import { DisplayError } from '@/lib/errors/displayError';

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

function DashboardPage() {
  const [error, setError] = useState<DisplayError | null>(null);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_isPopoverOpen, setIsPopoverOpen] = useState(false);

  const { data: result, isLoading } = useApiGetAllAccounts();

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

  const defaultCalendarCaption = (
    <div className="flex items-center gap-1">
      <CalendarIcon className="w-4 h-4 text-muted-foreground" />
      <span>Select the due date</span>
    </div>
  );

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
            selectedDate={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedCalendar
            caption={defaultCalendarCaption}
            selectedDate={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedCalendar
            selectedDate={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedCalendar
            caption="Select a date" // This one uses a string, just to show it works
            showBorder={false}
            compactHeader={false}
            selectedDate={selectedDate}
            onDateAccepted={handleDateAccepted}
            onCancel={handleCalendarCancel}
          />
          <EnrichedDatePicker
            caption={defaultCalendarCaption}
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
}

export default DashboardPage;
