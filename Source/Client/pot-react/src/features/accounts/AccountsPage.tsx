import { Outlet } from 'react-router';

import { AccountsHeader } from './AccountsHeader';
import { AccountsTable } from './AccountsTable';
// import {
//   Card,
//   CardContent,
//   CardDescription,
//   CardHeader,
//   CardTitle,
// } from '@/components/ui/card';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

  return (
    <div className="flex flex-col flex-1">
      <AccountsHeader />
      <div className="flex-1 p-8 pt-6">
        <AccountsTable />
        {/* EXPERIMENTAL CODE BELOW TO WORK OUT COLUMNS */}
        {/* <div className="mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Title</CardTitle>
              <CardDescription>Description</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4 w-full">
                <AccountsTable />
                <AccountsTable />
              </div>
            </CardContent>
          </Card>
        </div> */}
        {/* EXPERIMENTAL CODE ABOVE TO WORK OUT COLUMNS */}
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing accounts */}
      <Outlet />
    </div>
  );
};

export default AccountsPage;
