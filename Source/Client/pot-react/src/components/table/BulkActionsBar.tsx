import { Button } from '../ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';

/**
 * Represents a bulk action that can be performed on multiple selected items.
 *
 * @template TData - The type of the data items that the action operates on.
 */
type BulkAction<TData> = {
  /** Display label for the action (e.g., "Delete", "Export", "Auto Advance") */
  label: string;
  /** Function called when the action is triggered, receives all selected items */
  onClick: (selectedItems: TData[]) => void;
};

/**
 * Props for the BulkActionsBar component.
 *
 * @template TData - The type of the data items in the table.
 */
type BulkActionsBarProps<TData> = {
  /** Number of currently selected items */
  selectedCount: number;
  /** Array of currently selected data items */
  selectedItems: TData[];
  /** Array of available bulk actions */
  bulkActions: BulkAction<TData>[];
  /** Callback to clear all selections */
  onClearSelection: () => void;
  /** Whether the bulk actions bar should be visible (typically when row selection is enabled) */
  isVisible: boolean;
};

/**
 * BulkActionsBar - A reusable component for displaying bulk actions when items are selected in a table.
 *
 * This component provides a consistent interface for:
 * - Showing selection count ("X selected" or "No items selected")
 * - Clearing selection with a "Clear Selection" button
 * - Dropdown menu with custom bulk actions
 *
 * Features:
 * - Automatically hides when `isVisible` is false or no bulk actions are provided
 * - Disables action buttons when no items are selected
 * - Responsive design with proper hover states for light/dark themes
 * - Type-safe with full generic support for any data type
 *
 * Usage example:
 * ```tsx
 * const bulkActions: BulkAction<MyDataType>[] = [
 *   {
 *     label: "Delete Selected",
 *     onClick: (items) => deleteItems(items)
 *   },
 *   {
 *     label: "Export to CSV",
 *     onClick: (items) => exportToCsv(items)
 *   }
 * ];
 *
 * <BulkActionsBar
 *   selectedCount={selectedCount}
 *   selectedItems={selectedItems}
 *   bulkActions={bulkActions}
 *   onClearSelection={() => setSelection({})}
 *   isVisible={enableRowSelection}
 * />
 * ```
 *
 * @template TData - The type of data items that actions operate on
 */

function BulkActionsBar<TData>({
  selectedCount,
  selectedItems,
  bulkActions,
  onClearSelection,
  isVisible,
}: BulkActionsBarProps<TData>) {
  // Hide the component if it's not visible or no bulk actions are configured
  if (!isVisible || bulkActions.length === 0) {
    return null;
  }

  return (
    <div className="flex items-center gap-3 bg-muted p-3 rounded-md mb-4">
      {/* Selection count display with fixed width to prevent layout shifting */}
      <span className="text-sm text-muted-foreground w-32">
        {selectedCount > 0 ? `${selectedCount} selected` : 'No items selected'}
      </span>

      {/* Clear selection button - disabled when nothing is selected */}
      <Button
        variant="ghost"
        size="sm"
        onClick={onClearSelection}
        disabled={selectedCount === 0}
        className="hover:bg-slate-200 hover:text-slate-900 dark:hover:bg-slate-700 dark:hover:text-slate-100 transition-colors"
      >
        Clear Selection
      </Button>

      {/* Dropdown menu containing all available bulk actions */}
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={selectedCount === 0}
            className="hover:bg-slate-200 hover:text-slate-900 dark:hover:bg-slate-700 dark:hover:text-slate-100 transition-colors"
          >
            Actions
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start">
          {/* Render each bulk action as a dropdown menu item */}
          {bulkActions.map((action, index) => (
            <DropdownMenuItem
              key={index}
              onClick={() => action.onClick(selectedItems)}
              disabled={selectedCount === 0}
            >
              {action.label}
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}

export default BulkActionsBar;

// Export types for use in other components
export type { BulkAction, BulkActionsBarProps };
