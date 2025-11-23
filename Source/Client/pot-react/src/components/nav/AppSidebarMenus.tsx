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

import { SidebarContent } from '@/components/ui/sidebar';
import { useAuthContext } from '@/features/auth/contexts';
import { ExportModal } from '@/features/maintenance/export/components/ExportModal';
import { ImportModal } from '@/features/maintenance/import/components/ImportModal';

import type { MenuGroupDefinition } from './MenuGroup';
import MenuGroup from './MenuGroup';

function AppSidebarMenus() {
  const { isAuthenticated } = useAuthContext();
  const [isExportModalOpen, setIsExportModalOpen] = useState(false);
  const [isImportModalOpen, setIsImportModalOpen] = useState(false);

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
          label: 'Expenses',
          icon: Receipt,
          href: '/expenses',
          permissions: ['expense:view'],
        },
        {
          type: 'href',
          label: 'Income',
          icon: TrendingUp,
          href: '/incomes',
          permissions: ['income:view'],
        },
        {
          type: 'href',
          label: 'Accounts',
          icon: Landmark,
          href: '/accounts',
          permissions: ['account:view'],
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
      items: [
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
      <SidebarContent>
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
