import { create } from 'zustand';

import { User } from '@/data/user';

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

export { useUserStore };
