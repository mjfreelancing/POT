import { Outlet } from 'react-router';

import { ExpensesHeader, ExpensesTable } from './components';

function ExpensesPage() {
  console.info('Rendering ExpensesPage');

  return (
    <div className="flex flex-col flex-1 min-h-screen bg-gradient-to-br from-background to-muted/20">
      <ExpensesHeader />
      <div className="flex-1 p-6 space-y-6">
        <ExpensesTable />
      </div>
      {/* The Outlet is used to render nested routes, such as when creating/editing expenses */}
      <Outlet />
    </div>
  );
}

export default ExpensesPage;
