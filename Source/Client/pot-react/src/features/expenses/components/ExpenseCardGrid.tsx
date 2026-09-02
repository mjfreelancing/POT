import { Card, CardContent } from '@/components/ui/card';
import type { Expense } from '@/data';

import ExpenseMobileCard from './ExpenseMobileCard';

type ExpenseCardGridProps = {
  expenses: Expense[];
};

/**
 * Grid view for displaying expenses as cards on mobile devices.
 * Uses 2-column grid layout for optimal mobile viewing.
 */
function ExpenseCardGrid({ expenses }: ExpenseCardGridProps) {
  return (
    <>
      <Card className="card-elevated flex flex-col flex-1 min-h-0 py-2 gap-0">
        <CardContent className="px-4 py-2 flex-1 min-h-0 overflow-auto">
          <div className="grid grid-cols-2 gap-3">
            {expenses.map(expense => (
              <ExpenseMobileCard key={expense.rowId} expense={expense} />
            ))}
          </div>
        </CardContent>
      </Card>
    </>
  );
}

export default ExpenseCardGrid;
export type { ExpenseCardGridProps };
