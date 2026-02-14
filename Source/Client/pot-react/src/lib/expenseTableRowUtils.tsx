import type { Row } from '@tanstack/react-table';

import { NotePopover } from '@/components/feedback';
import type { Expense } from '@/data';

function getAdornedExpenseDescription(row: Row<Expense>) {
  return (
    <div className="flex items-center gap-2">
      {row.original.description}
      {row.original.note ? <NotePopover note={row.original.note} /> : null}
    </div>
  );
}

export { getAdornedExpenseDescription };
