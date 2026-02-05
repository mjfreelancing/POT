import { Zap } from 'lucide-react';

import { Card, CardContent } from '@/components/ui/card';
import { PermissionGuard } from '@/features/auth/components';
import RenewAccrueAllAction from '@/features/dashboard/actions/accruals/RenewAccrueAllAction';

import {
  AccrueAccountExpensesAction,
  RenewExpensesAction,
  RenewIncomesAction,
} from '../actions/accruals';
import { AccrualsProvider } from '../contexts/AccrualsContext';
import DashboardCardHeader from './DashboardCardHeader';

function QuickActions() {
  return (
    <Card>
      <DashboardCardHeader
        icon={<Zap className="h-5 w-5" aria-hidden="true" />}
        title="Quick Actions"
        description="One-Click actions for common tasks"
      />
      <CardContent className="px-4 -mt-2">
        <div>
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
            <AccrualsProvider>
              <PermissionGuard permissions={['expense:manage']} mode="all">
                <RenewExpensesAction />
              </PermissionGuard>

              <PermissionGuard permissions={['income:manage']} mode="all">
                <RenewIncomesAction />
              </PermissionGuard>

              <PermissionGuard
                permissions={['expense:manage', 'account:manage']}
                mode="all"
              >
                <AccrueAccountExpensesAction />
              </PermissionGuard>

              {/* expenses and incomes are renewed, and accounts are accrued */}
              <PermissionGuard
                permissions={[
                  'expense:manage',
                  'income:manage',
                  'account:manage',
                ]}
                mode="all"
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
