import { create } from 'zustand';

type AccountsSummary = {
  totalBalance: number;
  totalReserved: number;
  totalAvailable: number;
  totalDailyAccrual: number;
  setSummary: (
    totalBalance: number,
    totalReserved: number,
    totalAvailable: number,
    totalDailyAccrual: number,
  ) => void;
};

const accountsSummaryStore = create<AccountsSummary>(set => ({
  totalBalance: 0,
  totalReserved: 0,
  totalAvailable: 0,
  totalDailyAccrual: 0,
  setSummary: (
    totalBalance,
    totalReserved,
    totalAvailable,
    totalDailyAccrual,
  ) =>
    set({
      totalBalance,
      totalReserved,
      totalAvailable,
      totalDailyAccrual,
    }),
}));

export default accountsSummaryStore;
export type { AccountsSummary };
