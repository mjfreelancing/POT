import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
} from "@/components/ui/sidebar";
import ThemeToggle from "./theme/ThemeToggle";
import { ChartSpline, Home, LayoutDashboard } from "lucide-react";
import MenuGroup, { MenuGroupDefinition } from "./nav/MenuGroup";
import { Link } from "react-router-dom";
import { createMenuItem } from "./nav/utils/menuUtils";

{
  /*
     TODO: Once all menu items have been thought out, decide whether to keep the config here and use as-is,
           or move it somewhere else and make it available to AppSidebar(), possibly via App()
  */
}

const menuGroups: Record<string, MenuGroupDefinition> = {
  analysis: {
    label: "Analysis",
    items: [
      createMenuItem({
        label: "Dashboard",
        icon: LayoutDashboard,
        href: "/dashboard",
        // This item will be made active if the user navigates to the home page, matching
        // the router config where / and /dashboard both mount the <DashboardPage>.
        // TODO: Is it possible to determine this automatically somehow ??
        isHome: true,
      }),
      createMenuItem({
        label: "Projections",
        icon: ChartSpline,
        href: "/projections",
      }),
    ],
  },
};

export function AppSidebar() {
  return (
    <Sidebar collapsible="icon">
      {/* TODO: The header will be moved to a custom component once the CSS issue is sorted */}
      <SidebarHeader>
        {/* FIX: Need the p-2 to not be applied when the sidebar is collapsed */}
        <div className="flex items-center gap-2 p-2">
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
      </SidebarContent>
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}
