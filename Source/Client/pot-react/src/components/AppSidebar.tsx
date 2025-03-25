import {
  //ArrowDownUp,
  ChartSpline,
  CircleDollarSignIcon,
  Home,
  LayoutDashboard,
  PiggyBank,
} from 'lucide-react';
import { Link } from 'react-router';

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
} from '@/components/ui/sidebar';

import MenuGroup, { MenuGroupDefinition } from './nav/MenuGroup';
import ThemeToggle from './theme/ThemeToggle';

{
  /*
     TODO: Once all menu items have been thought out, decide whether to keep the config here and use as-is,
           or move it somewhere else and make it available to AppSidebar(), possibly via App()
  */
}

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
        label: 'Manage',
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

  expenses: {
    label: 'Expenses',
    items: [
      {
        label: 'Manage',
        icon: CircleDollarSignIcon,
        href: '/expenses',
      },
    ],
  },
};

export function AppSidebar() {
  return (
    <Sidebar collapsible="icon">
      {/* TODO: The header will be moved to a custom component once the CSS issue is sorted */}
      <SidebarHeader className="group">
        <div className="flex items-center gap-2 group-data-[state=collapsed]:p-0 p-2">
          <Link
            className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground"
            to="/"
          >
            <Home className="size-4" />
          </Link>
          {/*
              truncate' applies these CSS rules:
                white-space: nowrap;          => prevents wrapping
                overflow: hidden;             => hides the text if the container shrinks
                text-overflow: ellipsis;      => not applicable, but replaces the text with ... if space allows
            */}
          <div className="grid flex-1 text-left text-sm leading-tight">
            <span className="truncate font-semibold">Header content here</span>
          </div>
        </div>
      </SidebarHeader>
      <SidebarContent>
        <MenuGroup group={menuGroups.analysis} />
        <MenuGroup group={menuGroups.accounts} />
        <MenuGroup group={menuGroups.expenses} />
      </SidebarContent>
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}
