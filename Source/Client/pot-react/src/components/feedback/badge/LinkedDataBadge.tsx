import { CircleDollarSignIcon, ShoppingCart } from 'lucide-react';

import { Badge } from '@/components/ui/badge';

type LinkedDataType = 'expense' | 'income';

type LinkedDataBadgeProps = {
  type: LinkedDataType;
  count: number;
  className?: string; // additional, optional, class names for styling
};

const badgeConfig = {
  expense: {
    icon: ShoppingCart,
    className:
      'text-xs bg-red-50 text-red-700 border-red-200 dark:bg-red-950 dark:text-red-300 dark:border-red-800',
  },
  income: {
    icon: CircleDollarSignIcon,
    className:
      'text-xs bg-green-50 text-green-700 border-green-200 dark:bg-green-950 dark:text-green-300 dark:border-green-800',
  },
} as const;

function LinkedDataBadge({ type, count, className }: LinkedDataBadgeProps) {
  const config = badgeConfig[type];
  const Icon = config.icon;

  return (
    <Badge
      variant="outline"
      className={`${config.className} ${className || ''}`}
    >
      <Icon className="w-3 h-3" />
      {count}
    </Badge>
  );
}

export { LinkedDataBadge, type LinkedDataBadgeProps };
