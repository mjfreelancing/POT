import { Card, CardContent } from '@/components/ui/card';
import AccrualsAction from '@/features/dashboard/actions/accruals/AccrualsAction';

function QuickActions() {
  return (
    <Card>
      <CardContent className="px-4">
        <div>
          <h2 className="text-lg font-semibold mb-4">Quick Actions</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 max-w-2xl w-full">
            <AccrualsAction />
            {/* other actions can be added here */}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default QuickActions;
