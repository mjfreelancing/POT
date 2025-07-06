import { AppSidebarTrigger } from '@/components/nav';

function IncomesHeader() {
  return (
    <div className="page-header">
      <div className="flex items-center">
        <AppSidebarTrigger />
        <div className="flex items-center gap-3">
          <div>
            <h1 className="page-title">Income Management</h1>
            <p className="page-subtitle">
              Track and manage your income sources
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default IncomesHeader;
