import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
} from "@/components/ui/sidebar";
import ThemeToggle from "./theme/ThemeToggle";
import { ChartSpline, Home, LayoutDashboard } from "lucide-react";
import MenuGroup, { MenuGroupDefinition } from "./nav/MenuGroup";

const menuGroups: Record<string, MenuGroupDefinition> = {
  analysis: {
    label: "Analysis",
    items: [
      {
        label: "Dashboard",
        icon: LayoutDashboard,
        href: "#",
        // tooltip: - will be the same as 'label'
      },
      {
        label: "Projections",
        icon: ChartSpline,
        href: "#",
        // tooltip: - will be the same as 'label'
      },
    ],
  },
};

export function AppSidebar() {
  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <div className="flex items-center gap-2 p-2">
          <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
            <Home className="size-4" />
          </div>

          <div className="grid flex-1 text-left text-sm leading-tight">
            <span className="truncate font-semibold">Header here</span>
          </div>
        </div>
      </SidebarHeader>
      <SidebarContent>
        <MenuGroup group={menuGroups.analysis} />
      </SidebarContent>
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}
