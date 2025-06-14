import { ReactNode } from 'react';

import { Card, CardContent } from '@/components/ui/card';
import { cn } from '@/lib/utils';

/**
 * Props for the ActionCard component.
 */
type ActionCardProps = {
  /** The icon to display on the left side of the card. Can be any React element (typically a Lucide icon) */
  icon: ReactNode;
  /** The main title text displayed prominently in the card */
  title: string;
  /** Optional description text displayed below the title in muted colors */
  description?: string;
  /** Additional CSS classes to apply to the card for custom styling */
  className?: string;
  /** Optional additional content to display below the description */
  children?: ReactNode;
  /** Optional click handler. When provided, the card becomes visually interactive with hover effects, cursor pointer, and clickable behavior */
  onClick?: () => void;
};

/**
 * A reusable card component for displaying actionable content with an icon, title, and optional description.
 * The card automatically becomes visually interactive (hover effects, cursor pointer) when an onClick handler is provided.
 *
 * @param props - The component props
 * @param props.icon - The icon to display on the left side of the card. Can be any React element (typically a Lucide icon)
 * @param props.title - The main title text displayed prominently in the card
 * @param props.description - Optional description text displayed below the title in muted colors
 * @param props.className - Additional CSS classes to apply to the card for custom styling
 * @param props.children - Optional additional content to display below the description
 * @param props.onClick - Optional click handler. When provided, the card becomes visually interactive with hover effects, cursor pointer, and clickable behavior
 *
 * @example
 * // Basic card with icon and title
 * <ActionCard
 *   icon={<Plus className="h-5 w-5" />}
 *   title="Add New Item"
 *   description="Create a new item in the system"
 *   onClick={() => console.log('Card clicked')}
 * />
 *
 * @example
 * // Card with additional content
 * <ActionCard
 *   icon={<Settings className="h-5 w-5" />}
 *   title="Settings"
 *   description="Configure account preferences"
 * >
 *   <div>Some additional information</div>
 * </ActionCard>
 */
function ActionCard({
  icon,
  title,
  description,
  className,
  children,
  onClick,
}: ActionCardProps) {
  return (
    <Card
      className={cn(
        'border-slate-200 dark:border-muted bg-slate-50 dark:bg-background transition-all duration-200',
        onClick && [
          'cursor-pointer hover:shadow-lg hover:scale-[1.02] hover:border-primary/50 hover:shadow-primary/10',
        ],
        className,
      )}
      onClick={onClick}
    >
      <CardContent className="p-4">
        <div className="flex items-center gap-4">
          <div className="p-3 rounded-lg bg-primary/10 text-primary flex-shrink-0">
            {icon}
          </div>
          <div className="flex-1 min-w-0">
            <h4 className="font-medium">{title}</h4>
            {description && (
              <p className="text-sm text-muted-foreground">{description}</p>
            )}
            {children}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default ActionCard;
export type { ActionCardProps };
