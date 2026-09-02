import { Card, CardContent } from '@/components/ui/card';
import type { Income } from '@/data';

import IncomeMobileCard from './IncomeMobileCard';

type IncomeCardGridProps = {
  incomes: Income[];
};

/**
 * Grid view for displaying incomes as cards on mobile devices.
 * Uses 2-column grid layout for optimal mobile viewing.
 */
function IncomeCardGrid({ incomes }: IncomeCardGridProps) {
  return (
    <>
      <Card className="card-elevated flex flex-col flex-1 min-h-0 py-2 gap-0">
        <CardContent className="px-4 py-2 flex-1 min-h-0 overflow-auto">
          <div className="grid grid-cols-2 gap-3">
            {incomes.map(income => (
              <IncomeMobileCard key={income.rowId} income={income} />
            ))}
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default IncomeCardGrid;
export type { IncomeCardGridProps };
