import { Home } from 'lucide-react';
import { Link } from 'react-router';

import { SidebarHeader } from '@/components/ui/sidebar';

function AppSidebarHeader() {
  return (
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
          <span className="truncate font-semibold">Pay On Time</span>
        </div>
      </div>
    </SidebarHeader>
  );
}

export { AppSidebarHeader };
