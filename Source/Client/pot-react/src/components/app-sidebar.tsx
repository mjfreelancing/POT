import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
} from "@/components/ui/sidebar";
import ThemeToggle from "./theme/theme-toggle";
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@radix-ui/react-collapsible";
import { ChevronDown } from "lucide-react";

export function AppSidebar() {
  return (
    <Sidebar collapsible="icon">
      <SidebarHeader />
      <SidebarContent>
        <Collapsible defaultOpen className="group/collapsible">
          <SidebarGroup>
            {/* wrapped the CollapsibleTrigger in a SidebarGroupLabel to render a button. */}
            <SidebarGroupLabel asChild>
              <CollapsibleTrigger>
                Accounts
                <ChevronDown className="ml-auto transition-transform group-data-[state=open]/collapsible:rotate-180" />
              </CollapsibleTrigger>
            </SidebarGroupLabel>
            <CollapsibleContent>
              <div>View Accounts</div>
              <span className="sr-only">View Accounts</span>
            </CollapsibleContent>
          </SidebarGroup>
        </Collapsible>

        <Collapsible defaultOpen className="group/collapsible">
          <SidebarGroup>
            {/* wrapped the CollapsibleTrigger in a SidebarGroupLabel to render a button. */}
            <SidebarGroupLabel asChild>
              <CollapsibleTrigger>
                Expenses
                <ChevronDown className="ml-auto transition-transform group-data-[state=open]/collapsible:rotate-180" />
              </CollapsibleTrigger>
            </SidebarGroupLabel>
            <CollapsibleContent>
              <div>View Expenses</div>
              <span className="sr-only">View Expenses</span>
            </CollapsibleContent>
          </SidebarGroup>
        </Collapsible>
      </SidebarContent>
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}
