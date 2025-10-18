import type { ReactNode } from 'react';

import { CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

/**
 * Reusable dashboard card header with icon, title, and description.
 */
type DashboardCardHeaderProps = {
  icon: ReactNode;
  title: string;
  description: string;
};

function DashboardCardHeader({
  icon,
  title,
  description,
}: DashboardCardHeaderProps) {
  return (
    <CardHeader>
      <div className="flex items-center gap-4">
        <div className="p-2 rounded-lg bg-primary/10 text-primary">{icon}</div>
        <div>
          <CardTitle>{title}</CardTitle>
          <CardDescription className="mt-1">{description}</CardDescription>
        </div>
      </div>
    </CardHeader>
  );
}

export default DashboardCardHeader;
export type { DashboardCardHeaderProps };
