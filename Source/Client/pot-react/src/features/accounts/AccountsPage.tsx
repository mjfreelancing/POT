import { Outlet } from 'react-router';

import { AccountsHeader } from './AccountsHeader';
import { AccountsTable } from './AccountsTable';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

  return (
    <>
      <AccountsHeader />
      <div className="flex-1 p-8 pt-6">
        <AccountsTable />
      </div>
      <Outlet />
    </>
  );
};

export default AccountsPage;
