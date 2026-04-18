import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import BulkActionsBar, { type BulkAction } from '@/components/table/BulkActionsBar';

type TestItem = {
  id: string;
  name: string;
};

describe('BulkActionsBar', () => {
  test('renders nothing when not visible', () => {
    const { container } = render(
      <BulkActionsBar<TestItem>
        selectedCount={1}
        selectedItems={[{ id: '1', name: 'Alpha' }]}
        bulkActions={[
          {
            label: 'Archive',
            onClick: vi.fn(),
          },
        ]}
        isVisible={false}
      />,
    );

    expect(container.firstChild).toBeNull();
  });

  test('shows default selection state and disables actions button when nothing is selected', () => {
    render(
      <BulkActionsBar<TestItem>
        selectedCount={0}
        selectedItems={[]}
        bulkActions={[
          {
            label: 'Archive',
            onClick: vi.fn(),
          },
        ]}
        isVisible={true}
      />,
    );

    expect(screen.getByText('No items selected')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Open bulk actions menu' }),
    ).toBeDisabled();
  });

  test('runs visible enabled action and clears selection when configured', async () => {
    const user = userEvent.setup();
    const selectedItems = [{ id: '1', name: 'Alpha' }];
    const onActionClick = vi.fn();
    const onActionCompleted = vi.fn();

    const bulkActions: BulkAction<TestItem>[] = [
      {
        label: 'Archive',
        onClick: onActionClick,
        clearSelectionOnComplete: true,
      },
      {
        label: 'Hidden Action',
        onClick: vi.fn(),
        isHidden: () => true,
      },
    ];

    render(
      <BulkActionsBar<TestItem>
        selectedCount={1}
        selectedItems={selectedItems}
        bulkActions={bulkActions}
        isVisible={true}
        onActionCompleted={onActionCompleted}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Open bulk actions menu' }),
    );

    expect(screen.queryByText('Hidden Action')).not.toBeInTheDocument();

    await user.click(screen.getByRole('menuitem', { name: 'Archive' }));

    expect(onActionClick).toHaveBeenCalledWith(selectedItems);
    expect(onActionCompleted).toHaveBeenCalledTimes(1);
  });

  test('renders disabled action menu item when action is disabled', async () => {
    const user = userEvent.setup();

    render(
      <BulkActionsBar<TestItem>
        selectedCount={1}
        selectedItems={[{ id: '1', name: 'Alpha' }]}
        bulkActions={[
          {
            label: 'Disabled Action',
            onClick: vi.fn(),
            isDisabled: true,
          },
        ]}
        isVisible={true}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Open bulk actions menu' }),
    );

    expect(screen.getByRole('menuitem', { name: 'Disabled Action' })).toHaveAttribute(
      'aria-disabled',
      'true',
    );
  });
});
