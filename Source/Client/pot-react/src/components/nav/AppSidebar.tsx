import { Sidebar, SidebarFooter } from '@/components/ui/sidebar';

import ThemeToggle from '../theme/ThemeToggle';
import { AppSidebarHeader } from './AppSidebarHeader';
import { AppSidebarMenus } from './AppSidebarMenus';

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
