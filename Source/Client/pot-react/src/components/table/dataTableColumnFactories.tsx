import type { ColumnDef, Row } from '@tanstack/react-table';
import type { ReactNode } from 'react';

import { Badge } from '@/components/ui/badge';

import type { Frequency, MoneyValue } from '../../lib';
import {
  formatDate,
  formatMoneyValue,
  FrequencyDisplay,
  getDaysDue,
  getStatusBadgeClass,
  getTableBadgeClass,
} from '../../lib';
import DataTableColumnHeader from './DataTableColumnHeader';

// Parameter types for our column factory functions
type BaseColumnParams<TData> = {
  accessorKey: keyof TData & string;
  header: string;
  options?: Partial<ColumnDef<TData>>;
};

type MoneyColumnParams<TData> = BaseColumnParams<TData>;

type DateColumnParams<TData> = BaseColumnParams<TData> & {
  // Function type for providing custom null value handling in date columns.
  // When undefined is returned, no content is displayed for null dates.
  // When a string is returned, it's displayed with muted styling.
  getNullValue?: (row: Row<TData>) => string | undefined;
};

type FrequencyColumnParams<TData> = {
  countKey: keyof TData & string;
  frequencyKey: keyof TData & string;
  header: string;
  options?: Partial<ColumnDef<TData>>;
};

type NextDueStatusRow = {
  nextDue: string;
  endDate: string | null;
  excludeFromCalcs: boolean;
};

type RecurringEndDateRow = {
  endDate: string | null;
  frequency: Frequency;
  excludeFromCalcs: boolean;
};

type AccountDescriptionRow = {
  account?: {
    description?: string | null;
  } | null;
};

const MIN_WIDTH_TABLE_CELL_CLASS = 'min-w-[80px] inline-block';

const renderMinWidthTableCell = (content: ReactNode) => {
  return <span className={MIN_WIDTH_TABLE_CELL_CLASS}>{content}</span>;
};

// Gets the money value from a row.
const getMoneyValue = <TData,>(row: Row<TData>, key: string): MoneyValue => {
  return parseFloat(row.getValue(key));
};

// Formats a money value as a string.
const formatCellMoneyValue = <TData,>(row: Row<TData>, key: string) => {
  const value = getMoneyValue(row, key);
  return formatMoneyValue(value);
};

// Mapping of plural frequency values to their singular forms, equivalent to:
//
// const frequencySingularMap: { [K in Frequency]: string } = {
//   Days: 'Day',
//   Weeks: 'Week',
//   Months: 'Month',
//   EndOfMonth: 'End of Month',
//   Years: 'Year',
//   OneTime: 'One Time',
// };
const frequencySingularMap: Record<Frequency, string> = {
  Days: 'Day',
  Weeks: 'Week',
  Months: 'Month',
  EndOfMonth: 'End of Month',
  Years: 'Year',
  OneTime: 'One Time',
};

// Note: The trailing comma in <TData,> is necessary for TypeScript to differentiate it from a JSX element.

// Creates a column definition for a money value (right-aligned number) column.
//
// Using DataTableColumnHeader as decribed at https://ui.shadcn.com/docs/components/data-table#reusable-components
// for a sortable header with a title.
const createMoneyValueColumn = <TData,>(
  params: MoneyColumnParams<TData>,
): ColumnDef<TData> => {
  const { accessorKey, header, options = {} } = params;
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: accessorKey,
    accessorKey,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const formattedValue = formatCellMoneyValue(row, accessorKey);

      return renderMinWidthTableCell(formattedValue);
    },
    enableSorting,
    ...restOptions,
  };
};

const createDateColumn = <TData,>(
  params: DateColumnParams<TData>,
): ColumnDef<TData> => {
  const { accessorKey, header, getNullValue, options = {} } = params;
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: accessorKey,
    accessorKey,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const rawValue = row.getValue(accessorKey) as string | Date;

      if (rawValue === null) {
        if (getNullValue) {
          const nullContent = getNullValue(row);

          if (typeof nullContent === 'string' && nullContent.length > 0) {
            return nullContent;
          }
        }

        return null;
      }

      // Handles Date and strings in yyyy-MM-dd format.
      const formattedValue = formatDate(rawValue);

      return renderMinWidthTableCell(formattedValue);
    },
    enableSorting,
    ...restOptions,
  };
};

const createNextDueStatusColumn = <
  TData extends NextDueStatusRow,
>(): ColumnDef<TData> => ({
  id: 'nextDue',
  accessorKey: 'nextDue',
  header: ({ column }) => (
    <DataTableColumnHeader column={column} title="Next Due" />
  ),
  enableSorting: true,
  sortingFn: 'datetime',
  cell: ({ row }) => {
    const { nextDue, endDate, excludeFromCalcs } = row.original;
    const formattedDate = formatDate(nextDue);
    const daysDue = getDaysDue(nextDue);
    const isEnded = endDate ? getDaysDue(endDate) < 0 : false;

    let badge: ReactNode = null;
    if (excludeFromCalcs) {
      badge = (
        <Badge variant="secondary" className={getStatusBadgeClass('excluded')}>
          Excluded
        </Badge>
      );
    } else if (isEnded) {
      badge = (
        <Badge variant="secondary" className={getStatusBadgeClass('ended')}>
          Ended
        </Badge>
      );
    } else if (daysDue === 0) {
      badge = (
        <Badge variant="default" className={getStatusBadgeClass('due-today')}>
          Due Today
        </Badge>
      );
    } else if (daysDue < 0) {
      badge = (
        <Badge variant="destructive" className={getStatusBadgeClass('overdue')}>
          Overdue
        </Badge>
      );
    } else if (daysDue <= 7) {
      badge = (
        <Badge variant="default" className={getStatusBadgeClass('due-soon')}>
          Due Soon
        </Badge>
      );
    }

    return (
      <div className="flex items-center">
        {renderMinWidthTableCell(formattedDate)}
        {badge}
      </div>
    );
  },
});

const createRecurringEndDateColumn = <
  TData extends RecurringEndDateRow,
>(): ColumnDef<TData> => ({
  id: 'endDate',
  accessorKey: 'endDate',
  header: 'End Date',
  cell: ({ row }) => {
    const { endDate, frequency, excludeFromCalcs } = row.original;
    const isOneTime = frequency === 'OneTime';

    if (isOneTime) {
      return (
        <Badge
          variant="secondary"
          className={getTableBadgeClass(
            excludeFromCalcs ? 'slate' : 'pink',
            excludeFromCalcs ? 'filled' : 'outline',
          )}
        >
          One-time
        </Badge>
      );
    }

    if (!endDate) {
      return null;
    }

    return <span>{formatDate(endDate)}</span>;
  },
});

const createAccountDescriptionColumn = <
  TData extends AccountDescriptionRow,
>(): ColumnDef<TData> => ({
  id: 'accountDescription',
  header: 'Account',
  cell: ({ row }) => {
    const description = row.original.account?.description;

    return (
      <div className={!description ? 'text-muted-foreground' : ''}>
        {description ?? 'Not Assigned'}
      </div>
    );
  },
});

const createActionsColumn = <TData,>(
  renderActions: (item: TData) => ReactNode,
): ColumnDef<TData> => ({
  id: 'actions',
  cell: ({ row }) => {
    return (
      <div className="flex justify-end">{renderActions(row.original)}</div>
    );
  },
});

/**
 * Creates a column showing "<count> <frequency>" based on two keys.
 */
const createFrequencyColumn = <TData,>(
  params: FrequencyColumnParams<TData>,
): ColumnDef<TData> => {
  const { countKey, frequencyKey, header, options = {} } = params;
  const { enableSorting = false, ...restOptions } = options;

  return {
    id: `${frequencyKey}-${countKey}`,
    header: enableSorting
      ? ({ column }) => <DataTableColumnHeader column={column} title={header} />
      : () => <div className="uppercase">{header}</div>,
    cell: ({ row }) => {
      const count = row.original[countKey] as number;
      const freq = row.original[frequencyKey] as Frequency;
      // Check if item is excluded (only applicable to expenses/incomes)
      // Since this is a generic column factory, TData could be any type. We cast to a shape that includes
      // the excludeFromCalcs property (which Expense and Income have) to safely access it without type errors.
      // The !! coerces the value to boolean (undefined/null becomes false, true/false stays as is).
      const isExcluded = !!(row.original as { excludeFromCalcs?: boolean })
        .excludeFromCalcs;

      const frequencyBadgeWidthClass = 'w-[90px]';

      // Special case: for non-counted frequencies, display label only, no count.
      let displayValue;

      if (freq === 'OneTime' || freq === 'EndOfMonth') {
        displayValue = FrequencyDisplay[freq];
      } else {
        const frequencyLabel = count === 1 ? frequencySingularMap[freq] : freq;
        displayValue = `${count} ${frequencyLabel}`;
      }

      // For excluded items, use a neutral slate outline badge that remains readable.
      if (isExcluded) {
        return (
          <Badge
            variant="secondary"
            className={`${getTableBadgeClass(
              'slate',
              'outline',
            )} ${frequencyBadgeWidthClass} border-dashed dark:bg-slate-900 dark:text-slate-200 dark:border-slate-500`}
          >
            {displayValue}
          </Badge>
        );
      }

      // Color scheme for frequency badges
      const getBadgeClasses = (frequency: Frequency): string => {
        switch (frequency) {
          case 'Days':
            return `${getTableBadgeClass('blue', 'outline')} ${frequencyBadgeWidthClass}`;
          case 'Weeks':
            return `${getTableBadgeClass('green', 'outline')} ${frequencyBadgeWidthClass}`;
          case 'Months':
            return `${getTableBadgeClass('purple', 'outline')} ${frequencyBadgeWidthClass}`;
          case 'EndOfMonth':
            return `${getTableBadgeClass('purple', 'outline')} ${frequencyBadgeWidthClass} border-dashed`;
          case 'Years':
            return `${getTableBadgeClass('amber', 'outline')} ${frequencyBadgeWidthClass}`;
          case 'OneTime':
            return `${getTableBadgeClass('pink', 'outline')} ${frequencyBadgeWidthClass}`;
          default:
            return `${getTableBadgeClass('slate', 'outline')} ${frequencyBadgeWidthClass}`;
        }
      };

      return (
        <Badge variant="secondary" className={getBadgeClasses(freq)}>
          {displayValue}
        </Badge>
      );
    },
    enableSorting,
    ...restOptions,
  };
};

export {
  createAccountDescriptionColumn,
  createActionsColumn,
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createNextDueStatusColumn,
  createRecurringEndDateColumn,
  renderMinWidthTableCell,
};
export type {
  AccountDescriptionRow,
  BaseColumnParams,
  DateColumnParams,
  FrequencyColumnParams,
  MoneyColumnParams,
  NextDueStatusRow,
  RecurringEndDateRow,
};
