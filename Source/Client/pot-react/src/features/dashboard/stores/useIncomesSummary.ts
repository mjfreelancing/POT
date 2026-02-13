import { create } from 'zustand';

type IncomesSummary = {
  dueIn7Days: number;
  totalNext7Days: number;
  dueIn30Days: number;
  totalNext30Days: number;
  setSummary: (
    dueIn7Days: number,
    totalNext7Days: number,
    dueIn30Days: number,
    totalNext30Days: number,
  ) => void;
};

const incomesSummaryStore = create<IncomesSummary>(set => ({
  dueIn7Days: 0,
  totalNext7Days: 0,
  dueIn30Days: 0,
  totalNext30Days: 0,
  setSummary: (dueIn7Days, totalNext7Days, dueIn30Days, totalNext30Days) =>
    set({
      dueIn7Days: dueIn7Days,
      totalNext7Days: totalNext7Days,
      dueIn30Days: dueIn30Days,
      totalNext30Days: totalNext30Days,
    }),
}));

export default incomesSummaryStore;
export type { IncomesSummary };
