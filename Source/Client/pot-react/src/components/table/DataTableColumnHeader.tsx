import { Column } from '@tanstack/react-table';
import { ArrowUpDown, ChevronDown, ChevronUp } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { cn } from '@/lib';

type DataTableColumnHeaderProps<TData, TValue> = {
  column: Column<TData, TValue>;
  title: string;
  className?: string;
} & React.HTMLAttributes<HTMLDivElement>;

function DataTableColumnHeader<TData, TValue>({
  column,
  title,
  className,
}: DataTableColumnHeaderProps<TData, TValue>) {
  if (!column.getCanSort()) {
    return <div className={cn('uppercase', className)}>{title}</div>;
  }

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Button
        variant="ghost"
        size="sm"
        className="-ml-3 h-8 data-[state=open]:bg-accent"
        onClick={() => {
          const currentSort = column.getIsSorted();

          if (currentSort === false) {
            // Not sorted -> sort ascending
            column.toggleSorting(false);
          } else if (currentSort === 'asc') {
            // Ascending -> sort descending
            column.toggleSorting(true);
          } else {
            // Descending -> clear sorting (three-state sorting)
            column.clearSorting();
          }
        }}
      >
        <span className="uppercase">{title}</span>
        {column.getIsSorted() === 'desc' ? (
          <ChevronDown className="ml-2 h-4 w-4" />
        ) : column.getIsSorted() === 'asc' ? (
          <ChevronUp className="ml-2 h-4 w-4" />
        ) : (
          <ArrowUpDown className="ml-2 h-4 w-4" />
        )}
      </Button>
    </div>
  );
}

export default DataTableColumnHeader;
export type { DataTableColumnHeaderProps };
