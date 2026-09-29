import { create } from 'zustand';

type AccountsSummary = {
  totalBalance: number;
  totalReserved: number;
  totalAvailable: number;
  totalStableExpenseAccrual: number;
  setSummary: (
    totalBalance: number,
    totalReserved: number,
    totalAvailable: number,
    totalStableExpenseAccrual: number,
  ) => void;
};

const accountsSummaryStore = create<AccountsSummary>(set => ({
  totalBalance: 0,
  totalReserved: 0,
  totalAvailable: 0,
  totalStableExpenseAccrual: 0,
  setSummary: (
    totalBalance,
    totalReserved,
    totalAvailable,
    totalStableExpenseAccrual,
  ) =>
    set({
      totalBalance,
      totalReserved,
      totalAvailable,
      totalStableExpenseAccrual,
    }),
}));

export default accountsSummaryStore;
export type { AccountsSummary };
