import { X } from 'lucide-react';

import { DisplayError } from '@/lib';

import { Button } from '../../ui/button';

export type SheetErrorProps = DisplayError & {
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
   * justify-between - spaces flex items with space between them (content vs X button)
   *
   * flex - enables flexbox layout for centering content
   * justify-center - centers the content horizontally
   * flex-1 - allows content area to grow and fill available space
   *
   * w-full - ensures inner container takes full width up to max width
   * max-w-2xl - sets maximum width constraint for centered content
   *
   * text-sm - sets small font size
   * font-semibold - applies semi-bold font weight
   * text-white - sets text color to white
   * text-white/90 - sets text color to white with 90% opacity
   * ml-4 - adds left margin to separate X button from content
   */
}
function ErrorSheet({ title, description, onDismiss }: SheetErrorProps) {
  const descriptionLines = description.split('\n');

  return (
    <div className="fixed top-0 left-0 right-0 z-50 p-4 bg-destructive/95">
      <div className="flex items-start justify-between">
        <div className="flex justify-center flex-1">
          <div className="w-full max-w-2xl">
            <h3 className="text-sm font-semibold text-white">{title}</h3>
            <div className="mt-1 text-sm text-white/90">
              {descriptionLines.map((line, index) => (
                <div key={index}>{line}</div>
              ))}
            </div>
          </div>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className="text-white hover:text-white/90 ml-4"
          onClick={onDismiss}
        >
          <X className="h-4 w-4" />
          <span className="sr-only">Dismiss</span>
        </Button>
      </div>
    </div>
  );
}

export default ErrorSheet;
