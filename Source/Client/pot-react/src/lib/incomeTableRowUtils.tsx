import { NotePopover } from '@/components/feedback';
import type { AppRow } from '@/components/table';
import type { Income } from '@/data';

function getAdornedIncomeDescription(row: AppRow<Income>) {
  return (
    <div className="flex items-center gap-2">
      {row.original.description}
      {row.original.note ? <NotePopover note={row.original.note} /> : null}
    </div>
  );
}

export { getAdornedIncomeDescription };
