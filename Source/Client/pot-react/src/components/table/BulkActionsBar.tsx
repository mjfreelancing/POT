import React from 'react';

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
  /** Display label for the action (e.g., "Delete", "Export") */
  label: string;
  /** Function called when the action is triggered, receives all selected items */
  onClick: (selectedItems: TData[]) => void;
  /** Whether this action is currently disabled */
  isDisabled?: boolean;
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
  /** Whether the bulk actions bar should be visible (typically when row selection is enabled) */
  isVisible: boolean;
};

/**
 * BulkActionsBar - A reusable component for displaying bulk actions when items are selected in a table.
 *
 * This component provides a consistent interface for:
 * - Showing selection count ("X selected" or "No items selected")
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
  isVisible,
}: BulkActionsBarProps<TData>): React.ReactElement | null {
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

      {/* Dropdown menu containing all available bulk actions */}
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={selectedCount === 0}
            className="hover:bg-slate-200 hover:text-slate-900 dark:hover:bg-slate-700 dark:hover:text-slate-100 transition-colors"
            aria-label="Open bulk actions menu"
          >
            Actions
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start">
          {/* Render each bulk action as a dropdown menu item */}
          {bulkActions.map((action, index) => {
            const isDisabled = selectedCount === 0 || action.isDisabled;

            if (!isDisabled) {
              return (
                <DropdownMenuItem
                  key={index}
                  onClick={() => action.onClick(selectedItems)}
                >
                  {action.label}
                </DropdownMenuItem>
              );
            }

            return (
              <div key={index} className="cursor-not-allowed">
                <DropdownMenuItem
                  disabled={true}
                  aria-disabled={true}
                  className="pointer-events-none"
                >
                  {action.label}
                </DropdownMenuItem>
              </div>
            );
          })}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}

export default BulkActionsBar;

// Export types for use in other components
export type { BulkAction, BulkActionsBarProps };
