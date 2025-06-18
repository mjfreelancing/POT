import { create } from 'zustand';

type AccountsSummary = {
  totalBalance: number;
  totalReserved: number;
  totalAccrued: number;
  totalDailyAccrual: number;
  setSummary: (
    totalBalance: number,
    totalReserved: number,
    totalAccrued: number,
    totalDailyAccrual: number,
  ) => void;
};

const accountsSummaryStore = create<AccountsSummary>(set => ({
  totalBalance: 0,
  totalReserved: 0,
  totalAccrued: 0,
  totalDailyAccrual: 0,
  setSummary: (totalBalance, totalReserved, totalAccrued, totalDailyAccrual) =>
    set({
      totalBalance,
      totalReserved,
      totalAccrued,
      totalDailyAccrual,
    }),
}));

export default accountsSummaryStore;
export type { AccountsSummary };
