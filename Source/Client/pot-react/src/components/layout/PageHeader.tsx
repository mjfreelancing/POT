import { AppSidebarTrigger } from '@/components/nav';

import { UserMenu } from '../user/UserMenu';

type PageHeaderProps = {
  title: string;
  subtitle?: string;
  showSidebarTrigger?: boolean;
};

function PageHeader({
  title,
  subtitle,
  showSidebarTrigger = true,
}: PageHeaderProps) {
  return (
    <div className="page-header flex items-center w-full justify-between">
      {showSidebarTrigger && (
        <div className="shrink-0 mr-2">
          <AppSidebarTrigger />
        </div>
      )}
      <div className="flex-1">
        <h1 className="page-title">{title}</h1>
        {subtitle && (
          <p className="page-subtitle hidden md:block">{subtitle}</p>
        )}
      </div>
      <div className="ml-4">
        <UserMenu />
      </div>
    </div>
  );
}

export { PageHeader };
