import { JSX } from 'react';

import { AppSidebarTrigger } from '@/components/nav';

function ProjectionsHeader(): JSX.Element {
  return (
    <div className="page-header">
      <div className="flex items-center">
        <AppSidebarTrigger />
        <div>
          <h1 className="page-title">Projections</h1>
          <p className="page-subtitle">View your financial projections.</p>
        </div>
      </div>
    </div>
  );
}

export default ProjectionsHeader;
