import { MessageSquareMore, NotebookPen } from 'lucide-react';

import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';

/**
 * NotePopover displays a small icon that, when clicked, shows a popover with the note content.
 * Only renders if note is present.
 */
type NotePopoverProps = {
  note?: string | null;
  ariaLabel?: string;
  size?: number;
  className?: string;
};

function NotePopover({
  note,
  ariaLabel = 'Show note',
  size = 16,
  className,
}: NotePopoverProps) {
  if (!note) {
    return null;
  }

  return (
    <Popover>
      <PopoverTrigger asChild>
        <button
          type="button"
          className={`ml-2 text-information hover:text-primary ${className ?? ''}`.trim()}
          aria-label={ariaLabel}
        >
          <MessageSquareMore size={size} />
        </button>
      </PopoverTrigger>
      <PopoverContent className="w-[340px] text-sm whitespace-pre-line bg-gray-50 dark:bg-gray-900 shadow-lg border border-gray-200 dark:border-gray-800">
        <div className="flex items-center gap-2 mb-2 border-b pb-1 rounded-t px-2 py-1 shadow-sm bg-gray-100 dark:bg-gray-800">
          <NotebookPen size={22} className="text-information" />
          <span className="font-bold text-information tracking-wide uppercase text-sm">
            Note
          </span>
        </div>
        <div className="px-2 pt-2 text-foreground break-all">{note}</div>
      </PopoverContent>
    </Popover>
  );
}

export default NotePopover;
