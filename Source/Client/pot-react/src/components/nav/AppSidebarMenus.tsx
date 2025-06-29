import {
  ChartSpline,
  Landmark,
  LayoutDashboard,
  Receipt,
  TrendingUp,
} from 'lucide-react';

import { SidebarContent } from '@/components/ui/sidebar';

import MenuGroup, { MenuGroupDefinition } from './MenuGroup';

const menuGroups: Record<string, MenuGroupDefinition> = {
  analysis: {
    label: 'Analysis',
    items: [
      {
        label: 'Dashboard',
        icon: LayoutDashboard,
        href: '/dashboard',
      },
      {
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
        label: 'Expenses',
        icon: Receipt,
        href: '/expenses',
      },
      {
        label: 'Income',
        icon: TrendingUp,
        href: '/incomes',
      },
      {
        label: 'Accounts',
        icon: Landmark,
        href: '/accounts',
      },
    ],
  },
};

function AppSidebarMenus() {
  return (
    <SidebarContent>
      <MenuGroup group={menuGroups.analysis} />
      <MenuGroup group={menuGroups.manage} />
    </SidebarContent>
  );
}

export { AppSidebarMenus };
