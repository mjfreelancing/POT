import { CheckCircle, Clock, XCircle } from 'lucide-react';

import { Badge } from '@/components/ui/badge';
import type { UserStatus } from '@/data/siteUser';

type UserStatusBadgeProps = {
  status: UserStatus;
  className?: string;
};

export function UserStatusBadge({ status, className }: UserStatusBadgeProps) {
  const config = {
    Enabled: {
      className:
        'bg-green-100 text-green-800 border-green-200 min-w-[80px] justify-center',
      icon: CheckCircle,
      label: 'Enabled',
    },
    Disabled: {
      className:
        'bg-red-100 text-red-800 border-red-200 min-w-[80px] justify-center',
      icon: XCircle,
      label: 'Disabled',
    },
    Pending: {
      className:
        'bg-blue-100 text-blue-800 border-blue-200 min-w-[80px] justify-center',
      icon: Clock,
      label: 'Pending',
    },
  };

  const { className: statusClassName, icon: Icon, label } = config[status];

  return (
    <Badge className={`${statusClassName} ${className || ''}`}>
      <Icon className="h-3 w-3 mr-1" />
      {label}
    </Badge>
  );
}
