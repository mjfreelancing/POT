import { LogOut, Settings } from 'lucide-react';
import { useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { logoutManager } from '@/concerns';
import { useAuthContext } from '@/features/auth/contexts';
import { AccountSettingsSheet } from '@/features/userSettings/UserSettingsSheet';

function UserMenu() {
  const { userInfo } = useAuthContext();
  const [isProfileOpen, setIsProfileOpen] = useState(false);

  function handleProfileOpen(): void {
    setIsProfileOpen(true);
  }

  function handleProfileClose(): void {
    setIsProfileOpen(false);
  }

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="outline"
            className="h-9 rounded-lg border-border/80 bg-slate-100 px-3 shadow-sm transition-colors hover:bg-slate-200 dark:bg-slate-800/40 dark:hover:bg-slate-700/40 data-[state=open]:bg-slate-200 dark:data-[state=open]:bg-slate-700/40"
          >
            <span className="font-medium text-base md:text-lg">
              {userInfo?.displayName ?? 'User'}
            </span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={handleProfileOpen}>
            <Settings className="mr-2 size-4" />
            Settings
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuItem onClick={logoutManager.logout}>
            <LogOut className="mr-2 size-4" />
            Log Out
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      <AccountSettingsSheet
        open={isProfileOpen}
        onOpenChange={setIsProfileOpen}
        onClose={handleProfileClose}
      />
    </>
  );
}

export { UserMenu };
