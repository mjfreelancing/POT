import type { LucideIcon } from 'lucide-react';

type IconToastProps = {
  icon: LucideIcon;
  iconColor: string;
  title: string;
  description: string;
  details?: string;
};

function IconToast({
  icon: Icon,
  iconColor,
  title,
  description,
  details,
}: IconToastProps) {
  return (
    <div className="flex items-start">
      <Icon className={`${iconColor} mr-6 w-16 h-16`} />
      <div>
        <div className="text-xl font-semibold">{title}</div>
        <div className="mt-2 text-sm text-muted-foreground">{description}</div>
        {details && (
          <div className="mt-1 text-xs text-muted-foreground">{details}</div>
        )}
      </div>
    </div>
  );
}

export { IconToast };
export type { IconToastProps };
