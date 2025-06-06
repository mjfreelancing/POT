import { Outlet } from 'react-router';

import AccountsHeader from './AccountsHeader';
import AccountsTable from './AccountsTable';

function AccountsPage() {
  console.info('Rendering AccountsPage');

  return (
    <div className="flex flex-col flex-1">
      <AccountsHeader />
      <div className="flex-1 p-8 pt-6">
        <AccountsTable />
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing accounts */}
      <Outlet />
    </div>
  );
}

export default AccountsPage;
