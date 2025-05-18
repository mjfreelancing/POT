import { SidebarTrigger } from '@/components/ui/sidebar';

function AccountsHeader() {
  return (
    <div className="border-b">
      <div className="flex h-16 items-center px-4">
        <SidebarTrigger className="mr-2" />
        <h2 className="text-lg font-semibold">Account Management</h2>
        {/* TODO: Put real data here */}
        <div className="ml-auto mr-4 flex items-center">
          <div className="flex flex-col items-center text-sm">
            <span className="text-muted-foreground">Total Reserved</span>
            <span className="font-medium">$5,500.00</span>
          </div>
          <div className="ml-6 flex flex-col items-center text-sm">
            <span className="text-muted-foreground">Daily Accrual</span>
            <span className="font-medium">$42.09</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export { AccountsHeader };
