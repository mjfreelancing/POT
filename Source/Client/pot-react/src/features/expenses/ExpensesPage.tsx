import { Outlet } from 'react-router';

import ExpensesHeader from './ExpensesHeader';
import ExpensesTable from './ExpensesTable';

function ExpensesPage() {
  console.info('Rendering ExpensesPage');

  return (
    <div className="flex flex-col flex-1">
      <ExpensesHeader />
      <div className="flex-1 p-8 pt-6">
        <ExpensesTable />
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing expenses */}
      <Outlet />
    </div>
  );
}

export default ExpensesPage;
