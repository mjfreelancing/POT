import { SidebarTrigger } from '@/components/ui/sidebar';

function IncomesHeader() {
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

export default IncomesHeader;
