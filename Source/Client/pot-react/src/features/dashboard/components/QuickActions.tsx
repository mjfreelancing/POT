import { Zap } from 'lucide-react';

import { PermissionGuard } from '@/features/auth/components';

import { RenewExpensesAction, RenewIncomesAction } from '../actions/accruals';
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
      {/* Maintain 4 columns for desktop - allows for more actions to be added
          while maintaining a consistent layout */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        <AccrualsProvider>
          <PermissionGuard permissions={['expense:manage']} mode="all">
            <RenewExpensesAction />
          </PermissionGuard>

          <PermissionGuard permissions={['income:manage']} mode="all">
            <RenewIncomesAction />
          </PermissionGuard>
        </AccrualsProvider>
      </div>
    </CollapsibleSection>
  );
}

export default QuickActions;
