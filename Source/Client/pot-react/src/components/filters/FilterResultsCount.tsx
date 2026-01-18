import { Filter } from 'lucide-react';

import { Badge } from '@/components/ui/badge';

type FilterResultsCountProps = {
  filteredCount: number;
  totalCount: number;
};

/**
 * Displays a count of filtered vs total items.
 * Shows "X of Y items" when filtered, or just "Y items" when not filtered.
 */
function FilterResultsCount({
  filteredCount,
  totalCount,
}: FilterResultsCountProps) {
  const isFiltered = filteredCount !== totalCount;

  return (
    <Badge
      variant="secondary"
      className={
        isFiltered
          ? 'px-3 py-1.5 text-sm bg-blue-500/20 text-blue-700 dark:text-blue-300 border-blue-500/30'
          : 'px-3 py-1.5 text-sm'
      }
    >
      {isFiltered && <Filter className="h-3.5 w-3.5" />}
      {isFiltered && (
        <>
          Showing <span className="font-semibold">{filteredCount}</span> of{' '}
        </>
      )}
      <span className="font-semibold">{totalCount}</span> items
    </Badge>
  );
}

export type { FilterResultsCountProps };
export default FilterResultsCount;
