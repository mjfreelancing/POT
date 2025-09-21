import { LogOut, User } from 'lucide-react';
import { useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useAuth } from '@/features/auth/AuthContext';
import { ProfileSheet } from '@/features/profile/ProfileSheet';

function UserMenu() {
  const { userInfo, logout } = useAuth();
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
          <Button variant="ghost" className="flex items-center gap-2 px-3">
            <span className="font-medium text-base md:text-lg">
              {userInfo?.username ?? 'User'}
            </span>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={handleProfileOpen}>
            <User className="mr-2 size-4" />
            Profile
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuItem onClick={logout}>
            <LogOut className="mr-2 size-4" />
            Log Out
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      <ProfileSheet
        open={isProfileOpen}
        onOpenChange={setIsProfileOpen}
        onClose={handleProfileClose}
      />
    </>
  );
}

export { UserMenu };
