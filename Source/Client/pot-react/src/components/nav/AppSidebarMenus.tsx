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

  expenses: {
    label: 'Expenses',
    items: [
      {
        label: 'Manage Expenses',
        icon: Receipt,
        href: '/expenses',
      },
    ],
  },

  income: {
    label: 'Income',
    items: [
      {
        label: 'Manage Income',
        icon: TrendingUp,
        href: '/incomes',
      },
    ],
  },

  accounts: {
    label: 'Accounts',
    items: [
      {
        label: 'Manage Accounts',
        icon: Landmark,
        href: '/accounts',
      },
      // {
      //   label: 'Import/Export',
      //   icon: ArrowDownUp,
      //   href: '/accounts',
      // },
    ],
  },
};

function AppSidebarMenus() {
  return (
    <SidebarContent>
      <MenuGroup group={menuGroups.analysis} />
      <MenuGroup group={menuGroups.expenses} />
      <MenuGroup group={menuGroups.income} />
      <MenuGroup group={menuGroups.accounts} />
    </SidebarContent>
  );
}

export { AppSidebarMenus };
