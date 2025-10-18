import type { Row } from '@tanstack/react-table';
import { Ban } from 'lucide-react';

import { NotePopover, StatusBadge } from '@/components/feedback';
import type { Expense } from '@/data';

function getAdornedExpenseDescription(row: Row<Expense>) {
  return (
    <div className="flex items-center gap-2">
      {row.original.description}
      {row.original.note ? <NotePopover note={row.original.note} /> : null}
      {row.original.excludeFromCalcs && (
        <StatusBadge color="red" tooltip="Excluded from calculations">
          <Ban />
        </StatusBadge>
      )}
    </div>
  );
}

export { getAdornedExpenseDescription };
