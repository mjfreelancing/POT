import { create } from 'zustand';

import { UserInfo } from '@/api/types';

type UserStore = {
  userInfo: UserInfo | null;
  setUserInfo: (userInfo: UserInfo) => void;
  clearUserInfo: () => void;
};

const useUserStore = create<UserStore>(set => ({
  userInfo: null,
  setUserInfo: userInfo => set({ userInfo }),
  clearUserInfo: () => set({ userInfo: null }),
}));

export { useUserStore };
