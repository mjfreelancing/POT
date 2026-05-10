import {
  Archive,
  ArchiveRestore,
  ChartSpline,
  ClipboardCheck,
  Landmark,
  LayoutDashboard,
  Receipt,
  TrendingUp,
  Users,
} from 'lucide-react';
import { useState } from 'react';
import { useLocation } from 'react-router';

import { SidebarContent, useSidebar } from '@/components/ui/sidebar';
import { useAuthContext } from '@/features/auth/contexts';
import { ExportModal } from '@/features/maintenance/export';
import { ImportModal } from '@/features/maintenance/import';
import { useFeatureFilterHref } from '@/hooks/useFeatureFilterHref';

import type { MenuGroupDefinition } from './MenuGroup';
import MenuGroup from './MenuGroup';

function AppSidebarMenus() {
  const { isAuthenticated } = useAuthContext();
  const { isMobile } = useSidebar();
  const location = useLocation();
  const [isExportModalOpen, setIsExportModalOpen] = useState(false);
  const [isImportModalOpen, setIsImportModalOpen] = useState(false);

  // Build dynamic hrefs for feature-based navigation with account filters
  const expensesHref = useFeatureFilterHref('expenses');
  const incomesHref = useFeatureFilterHref('incomes');

  // Only show menu items when authenticated
  if (!isAuthenticated) {
    return <SidebarContent />;
  }

  const menuGroups: Record<string, MenuGroupDefinition> = {
    analysis: {
      label: 'Analysis',
      items: [
        {
          type: 'href',
          label: 'Dashboard',
          icon: LayoutDashboard,
          href: '/dashboard',
        },
        {
          type: 'href',
          label: 'Projections',
          icon: ChartSpline,
          href: '/projections',
          permissions: ['account:view'],
        },
      ],
    },

    manage: {
      label: 'Manage',
      items: [
        {
          type: 'href',
          label: 'Accounts',
          icon: Landmark,
          href: '/accounts',
          permissions: ['account:view'],
        },
        {
          type: 'href',
          label: 'Expenses',
          icon: Receipt,
          href: expensesHref,
          permissions: ['expense:view'],
        },
        {
          type: 'href',
          label: 'Income',
          icon: TrendingUp,
          href: incomesHref,
          permissions: ['income:view'],
        },
        {
          type: 'href',
          label: 'Users',
          icon: Users,
          href: '/users',
          permissions: ['user:manage', 'user:view'],
        },
      ],
    },

    platform: {
      label: 'Platform',
      items: [
        {
          type: 'href',
          label: 'Approvals',
          icon: ClipboardCheck,
          href: '/approvals/pending',
          permissions: ['platform:manage'],
        },
      ],
    },

    maintenance: {
      label: 'Maintenance',
      items: isMobile
        ? [] // Hide export/import on mobile as they don't work with mobile file systems
        : [
            {
              type: 'onClick',
              label: 'Export...',
              icon: Archive,
              onClick: () => setIsExportModalOpen(true),
              permissions: ['maintenance:export'],
            },
            {
              type: 'onClick',
              label: 'Import...',
              icon: ArchiveRestore,
              onClick: () => setIsImportModalOpen(true),
              permissions: ['maintenance:import'],
            },
          ],
    },

    // User menu group removed; user actions now in header
  };

  return (
    <>
      <SidebarContent key={location.pathname}>
        <MenuGroup group={menuGroups.analysis} />
        <MenuGroup group={menuGroups.manage} />
        <MenuGroup group={menuGroups.platform} />
        <MenuGroup group={menuGroups.maintenance} />
      </SidebarContent>

      <ExportModal
        isOpen={isExportModalOpen}
        onClose={() => setIsExportModalOpen(false)}
      />
      <ImportModal
        isOpen={isImportModalOpen}
        onClose={() => setIsImportModalOpen(false)}
      />
    </>
  );
}

export { AppSidebarMenus };
