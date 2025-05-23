import { Sidebar, SidebarFooter } from '@/components/ui/sidebar';

import ThemeToggle from '../theme/ThemeToggle';
import { AppSidebarMenus } from './AppSidebarMenus';
import { AppSidebarHeader } from './AppSidebarHeader';

function AppSidebar() {
  return (
    <Sidebar collapsible="icon">
      <AppSidebarHeader />
      <AppSidebarMenus />
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}

export { AppSidebar };
