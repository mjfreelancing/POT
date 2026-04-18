import { Card, CardContent } from '@/components/ui/card';

/**
 * Generic empty-state message used across feature pages.
 *
 * Actions are optional to support informational states where users
 * only need context and no immediate next-step button.
 */
type EmptyStateMessageBaseProps = {
  /** Primary empty-state heading. */
  title: string;
  /** Supporting explanation shown under the heading. */
  description: string;
};

type EmptyStateMessageNoActions = {
  primaryActionLabel?: never;
  onPrimaryAction?: never;
  secondaryActionLabel?: never;
  onSecondaryAction?: never;
};

type EmptyStateMessagePrimaryAction = {
  /** Optional label for the primary action. */
  primaryActionLabel: string;
  /** Handler for the primary action. */
  onPrimaryAction: () => void;
  secondaryActionLabel?: never;
  onSecondaryAction?: never;
};

type EmptyStateMessagePrimaryAndSecondaryActions = {
  /** Label for the primary action. */
  primaryActionLabel: string;
  /** Handler for the primary action. */
  onPrimaryAction: () => void;
  /** Label for the secondary action. */
  secondaryActionLabel: string;
  /** Handler for the secondary action. */
  onSecondaryAction: () => void;
};

type EmptyStateMessageProps = EmptyStateMessageBaseProps &
  (
    | EmptyStateMessageNoActions
    | EmptyStateMessagePrimaryAction
    | EmptyStateMessagePrimaryAndSecondaryActions
  );

/**
 * Renders an empty-state panel with an optional action row.
 *
 * Contract rules:
 * - Secondary CTA is only allowed when primary CTA is present.
 * - Each CTA requires both a label and handler.
 */
function EmptyStateMessage({
  title,
  description,
  primaryActionLabel,
  onPrimaryAction,
  secondaryActionLabel,
  onSecondaryAction,
}: EmptyStateMessageProps) {
  return (
    <Card className="card-elevated flex flex-col flex-1 min-h-0">
      <CardContent className="px-4 flex-1 min-h-0 flex items-center justify-center">
        <div className="text-center text-muted-foreground max-w-lg space-y-2">
          <p className="text-lg font-medium text-foreground">{title}</p>
          <p className="text-sm">{description}</p>
          {primaryActionLabel ? (
            <div className="pt-3 flex flex-wrap items-center justify-center gap-2">
              <button
                type="button"
                onClick={onPrimaryAction}
                className="text-sm font-medium text-primary hover:underline underline-offset-4"
              >
                {primaryActionLabel}
              </button>
              {secondaryActionLabel ? (
                <button
                  type="button"
                  onClick={onSecondaryAction}
                  className="text-sm font-medium text-muted-foreground hover:text-foreground hover:underline underline-offset-4"
                >
                  {secondaryActionLabel}
                </button>
              ) : null}
            </div>
          ) : null}
        </div>
      </CardContent>
    </Card>
  );
}

export default EmptyStateMessage;
export type { EmptyStateMessageProps };
