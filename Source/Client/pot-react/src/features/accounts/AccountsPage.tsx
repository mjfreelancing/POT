import { Outlet } from 'react-router';

import { AccountsHeader, AccountsTable } from './components';

function AccountsPage() {
  console.info('Rendering AccountsPage');

  return (
    <div className="flex flex-col flex-1 min-h-screen bg-gradient-to-br from-background to-muted/20">
      <AccountsHeader />
      <div className="flex-1 p-6 space-y-6">
        <AccountsTable />
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing accounts */}
      <Outlet />
    </div>
  );
}

export default AccountsPage;
