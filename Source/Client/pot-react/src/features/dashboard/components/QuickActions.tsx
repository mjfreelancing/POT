import { Card, CardContent } from '@/components/ui/card';
import AccrualsAction from '@/features/dashboard/actions/accruals/AccrualsAction';

function QuickActions() {
  return (
    <Card>
      <CardContent className="px-4">
        <div>
          <h2 className="text-lg font-semibold mb-4">Quick Actions</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <AccrualsAction />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default QuickActions;
