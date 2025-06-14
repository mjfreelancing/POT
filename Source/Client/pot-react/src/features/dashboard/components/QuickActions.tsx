import { BarChart3, Landmark, Receipt } from 'lucide-react';
import { useNavigate } from 'react-router';

import { ActionCard } from '@/components/cards';

function QuickActions() {
  const navigate = useNavigate();

  return (
    <div>
      <h2 className="text-lg font-semibold mb-4">Quick Actions</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <ActionCard
          title="Add Account"
          description="Create a new bank account"
          icon={<Landmark className="h-6 w-6" />}
          onClick={() => navigate('/accounts/create')}
        />
        <ActionCard
          title="Record Expense"
          description="Add a new expense entry"
          icon={<Receipt className="h-6 w-6" />}
          onClick={() => navigate('/expenses/create')}
        />
        <ActionCard
          title="View Reports"
          description="Analyze spending patterns"
          icon={<BarChart3 className="h-6 w-6" />}
          onClick={() => console.log('Reports navigation not yet implemented')}
        />
      </div>
    </div>
  );
}

export default QuickActions;
