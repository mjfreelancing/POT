import { ErrorSheet } from '@/components/feedback';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
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
  const { error, setError } = useErrorContext();

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
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
