import { create } from 'zustand';

import type { User } from '@/data/user';

type UserStore = {
  userInfo: User | null;
  setUserInfo: (userInfo: User) => void;
  clearUserInfo: () => void;
};

const useUserStore = create<UserStore>(set => ({
  userInfo: null,
  setUserInfo: userInfo => set({ userInfo }),
  clearUserInfo: () => set({ userInfo: null }),
}));

export default useUserStore;
