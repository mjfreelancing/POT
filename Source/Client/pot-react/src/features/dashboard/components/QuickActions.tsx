import { Zap } from 'lucide-react';

import { Card, CardContent } from '@/components/ui/card';
import { PermissionGuard } from '@/features/auth/components';
import RenewAccrueAllAction from '@/features/dashboard/actions/accruals/RenewAccrueAllAction';
import { AccrualsProvider } from '@/features/dashboard/contexts/AccrualsContext';

import {
  AccrueAccountExpensesAction,
  RenewExpensesAction,
  RenewIncomesAction,
} from '../actions/accruals';
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
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <AccrualsProvider>
              <PermissionGuard permission={['expense:manage']}>
                <RenewExpensesAction />
              </PermissionGuard>

              <PermissionGuard permission={['income:manage']}>
                <RenewIncomesAction />
              </PermissionGuard>

              <PermissionGuard
                permission={['expense:manage', 'account:manage']}
              >
                <AccrueAccountExpensesAction />
              </PermissionGuard>

              {/* expenses and incomes are renewed, and accounts are accrued */}
              <PermissionGuard
                permission={[
                  'expense:manage',
                  'income:manage',
                  'account:manage',
                ]}
              >
                <RenewAccrueAllAction />
              </PermissionGuard>
            </AccrualsProvider>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default QuickActions;
