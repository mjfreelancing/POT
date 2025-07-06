import { useNavigate } from 'react-router';

import { AppSidebarTrigger } from '@/components/nav';

function ExpensesHeader() {
  return (
    <div className="page-header">
      <div className="flex items-center">
        <AppSidebarTrigger />
        <div className="flex items-center gap-3">
          <div>
            <h1 className="page-title">Expense Management</h1>
            <p className="page-subtitle">Track and manage your expenses</p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ExpensesHeader;
