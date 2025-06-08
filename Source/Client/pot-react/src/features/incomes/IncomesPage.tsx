import { Outlet } from 'react-router';

import IncomesHeader from './IncomesHeader';
import IncomesTable from './IncomesTable';

function IncomesPage() {
  console.info('Rendering IncomesPage');

  return (
    <div className="flex flex-col flex-1">
      <IncomesHeader />
      <div className="flex-1 p-8 pt-6">
        <IncomesTable />
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing incomes */}
      <Outlet />
    </div>
  );
}

export default IncomesPage;
