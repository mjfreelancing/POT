import { X } from 'lucide-react';

import { DisplayError } from '@/lib/errors/displayError';

import { Button } from '../../ui/button';

type SheetErrorProps = DisplayError & {
  onDismiss: () => void;
};

{
  /* CSS Classes:
   * fixed - positions element relative to viewport
   * top-0, left-0, right-0 - anchors sheet to top of screen, spanning full width
   * z-50 - ensures high z-index to display above other content
   * p-4 - adds padding (1rem) to all sides
   * bg-destructive/95 - applies destructive background color with 95% opacity
   *
   * flex - enables flexbox layout
   * items-start - aligns flex items to start of cross axis
   * justify-between - spaces flex items with space between them
   *
   * flex-1 - allows content to grow and fill available space
   * text-sm - sets small font size
   * font-semibold - applies semi-bold font weight
   * text-white - sets text color to white
   * text-white/90 - sets text color to white with 90% opacity
   */
}
export const ErrorSheet = ({
  title,
  description,
  onDismiss,
}: SheetErrorProps) => {
  return (
    <div className="fixed top-0 left-0 right-0 z-50 p-4 bg-destructive/95">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <h3 className="text-sm font-semibold text-white">{title}</h3>
          <div className="mt-1 text-sm text-white/90">{description}</div>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className="text-white hover:text-white/90"
          onClick={onDismiss}
        >
          <X className="h-4 w-4" />
          <span className="sr-only">Dismiss</span>
        </Button>
      </div>
    </div>
  );
};
