import { create } from 'zustand';

type UserPermissionStore = {
  username: string | null;
  permissions: string[];
  setUserInfo: (username: string, permissions: string[]) => void;
  clearUserInfo: () => void;
};

const usePermissionStore = create<UserPermissionStore>(set => ({
  username: null,
  permissions: [],
  setUserInfo: (username, permissions) => set({ username, permissions }),
  clearUserInfo: () => set({ username: null, permissions: [] }),
}));

export { usePermissionStore };
