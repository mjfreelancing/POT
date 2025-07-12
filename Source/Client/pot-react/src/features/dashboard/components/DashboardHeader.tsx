import { AppSidebarTrigger } from '@/components/nav';

function DashboardHeader() {
  return (
    <div className="page-header">
      <div className="flex items-center">
        <AppSidebarTrigger />
        <div>
          <h1 className="page-title">Financial Dashboard</h1>
          <p className="page-subtitle">Overview of your financial status</p>
        </div>
      </div>
    </div>
  );
}

export default DashboardHeader;
