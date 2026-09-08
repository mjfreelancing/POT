import { NotePopover } from '@/components/feedback';
import type { AppRow } from '@/components/table';
import type { Expense } from '@/data';

function getAdornedExpenseDescription(row: AppRow<Expense>) {
  return (
    <div className="flex items-center gap-2">
      {row.original.description}
      {row.original.note ? <NotePopover note={row.original.note} /> : null}
    </div>
  );
}

export { getAdornedExpenseDescription };
