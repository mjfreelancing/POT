import { X } from 'lucide-react';

import { DisplayError } from '@/lib/errors/displayError';

import { Button } from '../../ui/button';

type SheetErrorProps = DisplayError & {
  onDismiss: () => void;
};

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
