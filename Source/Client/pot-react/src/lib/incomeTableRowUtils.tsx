import type { Row } from '@tanstack/react-table';

import { NotePopover } from '@/components/feedback';
import { Badge } from '@/components/ui/badge';
import type { Income } from '@/data';

function getAdornedIncomeDescription(row: Row<Income>) {
  return (
    <div className="flex items-center gap-2">
      {row.original.description}
      {row.original.note ? <NotePopover note={row.original.note} /> : null}
      {row.original.excludeFromCalcs && (
        <Badge
          variant="secondary"
          className="text-[10px] px-1.5 py-0 bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 border-red-300 dark:border-red-800"
        >
          Excluded
        </Badge>
      )}
    </div>
  );
}

export { getAdornedIncomeDescription };
