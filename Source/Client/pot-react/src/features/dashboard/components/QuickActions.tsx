import { Zap } from 'lucide-react';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import AccrualsAction from '@/features/dashboard/actions/accruals/AccrualsAction';

function QuickActions() {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-4">
          <div className="p-2 rounded-lg bg-primary/10 text-primary">
            <Zap className="h-5 w-5" />
          </div>
          <div>
            <CardTitle>Quick Actions</CardTitle>
            <CardDescription className="mt-1">Quick actions</CardDescription>
          </div>
        </div>
      </CardHeader>
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
