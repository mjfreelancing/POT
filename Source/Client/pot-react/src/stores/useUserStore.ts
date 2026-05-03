import { create } from 'zustand';
import { persist } from 'zustand/middleware';

import { buildEnvScopedKey, purgeLegacyStorageKeys } from '@/concerns/storage';
import type { User } from '@/data/user';

type UserStore = {
  userInfo: User | null;
  setUserInfo: (userInfo: User) => void;
  clearUserInfo: () => void;
};

// TEMPORARY: remove legacy pot-user key so stale data does not persist under the old key.
purgeLegacyStorageKeys([{ key: 'pot-user', storage: localStorage }]);

// Persist user info to localStorage to survive page refreshes.
// This ensures permissions are available immediately on reload, preventing
// PermissionGuard from blocking component rendering while /me endpoint is fetching.
// If /me fails (e.g., rate limiting), the app can still function with cached user data.
const useUserStore = create<UserStore>()(
  persist(
    set => ({
      userInfo: null,
      setUserInfo: userInfo => set({ userInfo }),
      clearUserInfo: () => set({ userInfo: null }),
    }),
    {
      name: buildEnvScopedKey('user'),
      // Only persist userInfo, exclude functions
      partialize: state => ({ userInfo: state.userInfo }),
    },
  ),
);

export default useUserStore;
