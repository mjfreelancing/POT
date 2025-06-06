import { create } from 'zustand';

export type AccountsSummary = {
  totalBalance: number;
  totalReserved: number;
  totalAllocated: number;
  totalDailyAccrual: number;
  setSummary: (
    totalBalance: number,
    totalReserved: number,
    totalAllocated: number,
    totalDailyAccrual: number,
  ) => void;
};

const accountsSummaryStore = create<AccountsSummary>(set => ({
  totalBalance: 0,
  totalReserved: 0,
  totalAllocated: 0,
  totalDailyAccrual: 0,
  setSummary: (
    totalBalance,
    totalReserved,
    totalAllocated,
    totalDailyAccrual,
  ) => set({ totalBalance, totalReserved, totalAllocated, totalDailyAccrual }),
}));

export default accountsSummaryStore;
