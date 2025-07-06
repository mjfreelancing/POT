import { AppSidebarTrigger } from '@/components/nav';

function AccountsHeader() {
  return (
    <div className="page-header">
      <div className="flex items-center">
        <AppSidebarTrigger />
        <div>
          <h1 className="page-title">Account Management</h1>
          <p className="page-subtitle">
            Monitor and manage your financial accounts
          </p>
        </div>
      </div>
    </div>
  );
}

export default AccountsHeader;
