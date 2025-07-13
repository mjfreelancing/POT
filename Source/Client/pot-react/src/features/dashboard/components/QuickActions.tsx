import { Zap } from 'lucide-react';

import { Card, CardContent } from '@/components/ui/card';
import AccrualsAction from '@/features/dashboard/actions/accruals/AccrualsAction';

import DashboardCardHeader from './DashboardCardHeader';

function QuickActions() {
  return (
    <Card>
      <DashboardCardHeader
        icon={<Zap className="h-5 w-5" aria-hidden="true" />}
        title="Quick Actions"
        description="Quick actions"
      />
      <CardContent className="px-4 -mt-2">
        <div>
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
