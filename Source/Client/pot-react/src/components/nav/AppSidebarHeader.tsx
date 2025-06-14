import { PiggyBank } from 'lucide-react';
import { Link } from 'react-router';

import { SidebarHeader } from '@/components/ui/sidebar';

/**
 * Application sidebar header component that displays the app logo and branding
 *
 * This component renders the header section of the application sidebar, featuring:
 * - A clickable logo/icon that navigates to the home page
 * - App title and subtitle with gradient styling
 * - Responsive behavior that adapts to sidebar collapsed/expanded states
 * - Smooth transitions and hover effects
 *
 * The component automatically adjusts its layout based on the sidebar's state:
 * - **Expanded state**: Shows both logo and text branding side by side
 * - **Collapsed state**: Shows only the logo, centered, with reduced size
 *
 * The logo uses a gradient background with a PiggyBank icon, reflecting the
 * financial management theme of the "Pay On Time" application.
 *
 * @returns JSX element containing the sidebar header with logo and branding
 *
 * @example
 * ```tsx
 * // Used within the main sidebar component
 * <Sidebar>
 *   <AppSidebarHeader />
 *   <AppSidebarMenus />
 * </Sidebar>
 * ```
 */
function AppSidebarHeader() {
  return (
    <SidebarHeader className="group">
      <div className="flex items-center gap-3 group-data-[state=collapsed]:justify-center group-data-[state=collapsed]:p-1 p-2">
        <Link
          className="flex aspect-square size-10 group-data-[state=collapsed]:size-8 items-center justify-center rounded-xl bg-blue-500 text-white shadow-md hover:shadow-lg transition-all"
          to="/"
        >
          <PiggyBank className="size-6 group-data-[state=collapsed]:size-5" />
        </Link>
        {/*
              truncate' applies these CSS rules:
                white-space: nowrap;          => prevents wrapping
                overflow: hidden;             => hides the text if the container shrinks
                text-overflow: ellipsis;      => not applicable, but replaces the text with ... if space allows
            */}
        <div className="grid flex-1 text-left leading-tight group-data-[state=collapsed]:hidden">
          <span className="truncate text-lg font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Pay On Time
          </span>
          <span className="truncate text-xs text-muted-foreground">
            Financial Management
          </span>
        </div>
      </div>
    </SidebarHeader>
  );
}

export { AppSidebarHeader };
