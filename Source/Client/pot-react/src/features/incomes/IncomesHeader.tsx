import { SidebarTrigger } from '@/components/ui/sidebar';
//import { formatMoneyValue } from '@/lib/money/moneyUtils';

// import {
//   AccountsSummary,
//   useAccountsSummary,
// } from './hooks/useAccountsSummary';

// type SummaryItemProps = {
//   title: string;
//   value: number;
// };

// function SummaryItem({ title, value }: SummaryItemProps) {
//   return (
//     <div className="flex flex-col items-center text-sm w-24">
//       <span className="text-muted-foreground">{title}</span>
//       <span className="font-medium">{formatMoneyValue(value)}</span>
//     </div>
//   );
// }

function IncomesHeader() {
  // const totalBalance = useAccountsSummary(
  //   (state: AccountsSummary) => state.totalBalance,
  // );

  // const totalReserved = useAccountsSummary(
  //   (state: AccountsSummary) => state.totalReserved,
  // );

  // const totalAllocated = useAccountsSummary(
  //   (state: AccountsSummary) => state.totalAllocated,
  // );

  // const totalDailyAccrual = useAccountsSummary(
  //   (state: AccountsSummary) => state.totalDailyAccrual,
  // );

  return (
    <div className="border-b">
      <div className="flex h-16 items-center px-4">
        <SidebarTrigger className="mr-2" />
        <h2 className="text-lg font-semibold">Income Management</h2>
        <div className="flex items-center space-x-8 ml-16">
          {/* <SummaryItem title="Balance" value={totalBalance} />
          <SummaryItem title="Reserved" value={totalReserved} />
          <SummaryItem title="Allocated" value={totalAllocated} />
          <SummaryItem title="Daily Accrual" value={totalDailyAccrual} /> */}
        </div>
      </div>
    </div>
  );
}

export { IncomesHeader };
