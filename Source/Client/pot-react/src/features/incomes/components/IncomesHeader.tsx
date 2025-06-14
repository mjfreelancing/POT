import { Plus } from 'lucide-react';
import { useNavigate } from 'react-router';

import { AppSidebarTrigger } from '@/components/nav';
import { Button } from '@/components/ui/button';

function IncomesHeader() {
  const navigate = useNavigate();

  return (
    <div className="page-header">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <AppSidebarTrigger />
          <div className="flex items-center gap-3">
            <div>
              <h1 className="page-title">Income Management</h1>
              <p className="page-subtitle">
                Track and manage your income sources
              </p>
            </div>
          </div>
        </div>
        <div className="flex items-center space-x-4">
          <Button
            onClick={() => navigate('create')}
            aria-label="Add a new income source"
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            Add Income
          </Button>
        </div>
      </div>
    </div>
  );
}

export default IncomesHeader;
