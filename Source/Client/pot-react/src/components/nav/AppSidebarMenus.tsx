import {
  Archive,
  ArchiveRestore,
  ChartSpline,
  Landmark,
  LayoutDashboard,
  LogOut,
  Receipt,
  TrendingUp,
} from 'lucide-react';
import { useState } from 'react';

import { SidebarContent } from '@/components/ui/sidebar';
import { useAuth } from '@/features/auth/AuthContext';
import { ExportModal } from '@/features/export/components/ExportModal';
import { ImportModal } from '@/features/import/components/ImportModal';

import MenuGroup, { MenuGroupDefinition } from './MenuGroup';

function AppSidebarMenus() {
  const { logout, isAuthenticated } = useAuth();
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
        },
        {
          type: 'href',
          label: 'Income',
          icon: TrendingUp,
          href: '/incomes',
        },
        {
          type: 'href',
          label: 'Accounts',
          icon: Landmark,
          href: '/accounts',
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
        },
        {
          type: 'onClick',
          label: 'Import...',
          icon: ArchiveRestore,
          onClick: () => setIsImportModalOpen(true),
        },
      ],
    },

    user: {
      label: 'User',
      items: [
        {
          type: 'onClick',
          label: 'Log Out',
          icon: LogOut,
          onClick: logout,
        },
      ],
    },
  };

  return (
    <>
      <SidebarContent>
        <MenuGroup group={menuGroups.analysis} />
        <MenuGroup group={menuGroups.manage} />
        <MenuGroup group={menuGroups.maintenance} />
        <MenuGroup group={menuGroups.user} />
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
