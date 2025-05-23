import { Sidebar, SidebarFooter } from '@/components/ui/sidebar';

import ThemeToggle from '../theme/ThemeToggle';
import { AppSidebarContent } from './AppSidebarContent';
import { AppSidebarHeader } from './AppSidebarHeader';

function AppSidebar() {
  return (
    <Sidebar collapsible="icon">
      <AppSidebarHeader />
      <AppSidebarContent />
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}

export { AppSidebar };
