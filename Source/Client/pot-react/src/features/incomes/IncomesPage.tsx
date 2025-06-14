import { Outlet } from 'react-router';

import { IncomesHeader, IncomesTable } from './components';

function IncomesPage() {
  console.info('Rendering IncomesPage');

  return (
    <div className="flex flex-col flex-1 min-h-screen bg-gradient-to-br from-background to-muted/20">
      <IncomesHeader />
      <div className="flex-1 p-6 space-y-6">
        <IncomesTable />
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing incomes */}
      <Outlet />
    </div>
  );
}

export default IncomesPage;
