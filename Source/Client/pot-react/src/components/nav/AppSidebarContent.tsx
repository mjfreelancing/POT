import {
  ChartSpline,
  CircleDollarSignIcon,
  LayoutDashboard,
  PiggyBank,
  ShoppingCart,
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

  accounts: {
    label: 'Accounts',
    items: [
      {
        label: 'Manage Accounts',
        icon: PiggyBank,
        href: '/accounts',
      },
      // {
      //   label: 'Import/Export',
      //   icon: ArrowDownUp,
      //   href: '/accounts',
      // },
    ],
  },

  income: {
    label: 'Income',
    items: [
      {
        label: 'Manage Income',
        icon: CircleDollarSignIcon,
        href: '/income',
      },
    ],
  },

  expenses: {
    label: 'Expenses',
    items: [
      {
        label: 'Manage Expenses',
        icon: ShoppingCart,
        href: '/expenses',
      },
    ],
  },
};

function AppSidebarContent() {
  return (
    <SidebarContent>
      <MenuGroup group={menuGroups.analysis} />
      <MenuGroup group={menuGroups.accounts} />
      <MenuGroup group={menuGroups.income} />
      <MenuGroup group={menuGroups.expenses} />
    </SidebarContent>
  );
}

export { AppSidebarContent };
