import { CheckCircle, Clock, XCircle } from 'lucide-react';

import { Badge } from '@/components/ui/badge';
import type { UserStatus } from '@/data/siteUser';

type UserStatusBadgeProps = {
  status: UserStatus;
  className?: string;
  compact?: boolean;
};

export function UserStatusBadge({
  status,
  className,
  compact = false,
}: UserStatusBadgeProps) {
  // Approval status users should not appear in site user tables
  // They are managed separately in the platform approvals page
  if (status === 'Approval') {
    return null;
  }

  const config = {
    Enabled: {
      className:
        'bg-green-100 text-green-800 border-green-200 min-w-[80px] justify-center',
      compactClassName:
        'bg-green-100 text-green-800 border-green-200 w-6 h-6 rounded-full p-0 justify-center items-center',
      icon: CheckCircle,
      label: 'Enabled',
    },
    Disabled: {
      className:
        'bg-red-100 text-red-800 border-red-200 min-w-[80px] justify-center',
      compactClassName:
        'bg-red-100 text-red-800 border-red-200 w-6 h-6 rounded-full p-0 justify-center items-center',
      icon: XCircle,
      label: 'Disabled',
    },
    Pending: {
      className:
        'bg-orange-100 text-orange-800 border-orange-200 min-w-[80px] justify-center',
      compactClassName:
        'bg-orange-100 text-orange-800 border-orange-200 w-6 h-6 rounded-full p-0 justify-center items-center',
      icon: Clock,
      label: 'Pending',
    },
  };

  const {
    className: statusClassName,
    compactClassName,
    icon: Icon,
    label,
  } = config[status];

  if (compact) {
    return (
      <Badge className={`${compactClassName} ${className || ''}`} title={label}>
        <Icon className="h-3.5 w-3.5" />
      </Badge>
    );
  }

  return (
    <Badge className={`${statusClassName} ${className || ''}`}>
      <Icon className="h-3 w-3 mr-1" />
      {label}
    </Badge>
  );
}
