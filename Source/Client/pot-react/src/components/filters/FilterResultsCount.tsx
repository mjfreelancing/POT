import { Filter } from 'lucide-react';

import { Badge } from '@/components/ui/badge';

type FilterResultsCountProps = {
  filteredCount: number;
  totalCount: number;
  /** Whether filtering is active (e.g., search text entered), regardless of whether it changed the count */
  isFilterActive?: boolean;
};

/**
 * Displays a count of filtered vs total items.
 * Always shows "Showing X of Y items" format to maintain consistent layout.
 * Highlights in blue when actively filtered.
 */
function FilterResultsCount({
  filteredCount,
  totalCount,
  isFilterActive,
}: FilterResultsCountProps) {
  // Use isFilterActive if provided, otherwise fall back to count comparison
  const isFiltered = isFilterActive ?? filteredCount !== totalCount;

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
      Showing <span className="font-semibold">{filteredCount}</span> of{' '}
      <span className="font-semibold">{totalCount}</span> items
    </Badge>
  );
}

export type { FilterResultsCountProps };
export default FilterResultsCount;
