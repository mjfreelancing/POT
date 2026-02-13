import { Zap } from 'lucide-react';

import { PermissionGuard } from '@/features/auth/components';
import RenewAccrueAllAction from '@/features/dashboard/actions/accruals/RenewAccrueAllAction';

import {
  AccrueAccountExpensesAction,
  RenewExpensesAction,
  RenewIncomesAction,
} from '../actions/accruals';
import { AccrualsProvider } from '../contexts/AccrualsContext';
import CollapsibleSection from './CollapsibleSection';

type QuickActionsProps = {
  isOpen: boolean;
  onOpenChange: (isOpen: boolean) => void;
};

function QuickActions({ isOpen, onOpenChange }: QuickActionsProps) {
  return (
    <CollapsibleSection
      icon={<Zap className="h-5 w-5" aria-hidden="true" />}
      title="Quick Actions"
      isOpen={isOpen}
      onOpenChange={onOpenChange}
    >
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
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
            permissions={['expense:manage', 'income:manage', 'account:manage']}
            mode="all"
          >
            <RenewAccrueAllAction />
          </PermissionGuard>
        </AccrualsProvider>
      </div>
    </CollapsibleSection>
  );
}

export default QuickActions;
