import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useForm } from 'react-hook-form';
import { describe, expect, test, vi } from 'vitest';

import ExpenseForm from '@/features/expenses/components/ExpenseForm';
import type { ExpenseFormData } from '@/features/expenses/schemas/expenseFormSchema';
import { AccrualPolicy, Frequency } from '@/lib';

import { createAccountWithIdentity } from '../../../shared/factories/accountFactory';

type ExpenseHarnessProps = {
  isEditMode?: boolean;
  onSubmit?: (values: ExpenseFormData) => Promise<void>;
  onCancel?: () => void;
  defaultValues?: Partial<ExpenseFormData>;
};

function ExpenseFormHarness({
  isEditMode = false,
  onSubmit = async () => {},
  onCancel = vi.fn(),
  defaultValues,
}: ExpenseHarnessProps) {
  const form = useForm<ExpenseFormData>({
    defaultValues: {
      excludeFromCalcs: false,
      description: 'Internet',
      nextDue: '2026-01-10',
      accrualPolicy: AccrualPolicy.Automatic,
      accrualStart: '2026-01-01',
      endDate: '2026-02-10',
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 120,
      note: '',
      accountRowId: 'account-1',
      ...defaultValues,
    },
  });

  return (
    <ExpenseForm
      form={form}
      onSubmit={onSubmit}
      onCancel={onCancel}
      submitLabel="Save"
      accounts={[createAccountWithIdentity('account-1', 'Main account')]}
      isEditMode={isEditMode}
    />
  );
}

async function selectFrequency(value: 'One Time' | 'Months') {
  await userEvent.click(screen.getByLabelText('Frequency'));
  await userEvent.click(await screen.findByRole('option', { name: value }));
}

describe('ExpenseForm', () => {
  test('clears and disables end date and normalizes frequencyCount when selecting One Time, then re-enables and restores recurring count', async () => {
    render(<ExpenseFormHarness />);

    const endDateButton = document.getElementById(
      'endDate-picker',
    ) as HTMLButtonElement;
    const frequencyCountInput = screen.getByLabelText('Every');

    expect(endDateButton).toBeEnabled();
    expect(endDateButton).not.toHaveTextContent('Pick a date');
    expect(frequencyCountInput).toHaveValue(1);

    await selectFrequency('One Time');

    await waitFor(() => {
      expect(endDateButton).toBeDisabled();
      expect(endDateButton).toHaveTextContent('Pick a date');
      expect(frequencyCountInput).toHaveValue(0);
    });

    await selectFrequency('Months');

    await waitFor(() => {
      expect(endDateButton).toBeEnabled();
      expect(endDateButton).toHaveTextContent('Pick a date');
      expect(frequencyCountInput).toHaveValue(1);
    });
  });

  test('sets focus to amount input in edit mode', async () => {
    render(<ExpenseFormHarness isEditMode={true} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Amount')).toHaveFocus();
    });
  });

  test('clears end date when Clear action is clicked', async () => {
    render(<ExpenseFormHarness />);

    const endDateButton = document.getElementById(
      'endDate-picker',
    ) as HTMLButtonElement;

    await userEvent.click(screen.getByRole('button', { name: 'Clear' }));

    await waitFor(() => {
      expect(endDateButton).toBeEnabled();
      expect(endDateButton).toHaveTextContent('Pick a date');
    });
  });

  test('clears and restores accrual start when accrual policy changes', async () => {
    render(<ExpenseFormHarness />);

    const accrualStartButton = document.getElementById(
      'accrualStart-picker',
    ) as HTMLButtonElement;

    expect(accrualStartButton).toBeEnabled();
    expect(accrualStartButton).not.toHaveTextContent('Pick a date');

    await userEvent.click(screen.getByLabelText('Accrual Policy'));
    await userEvent.click(await screen.findByRole('option', { name: 'None' }));

    await waitFor(() => {
      expect(accrualStartButton).toBeDisabled();
      expect(accrualStartButton).toHaveTextContent('Pick a date');
    });

    await userEvent.click(screen.getByLabelText('Accrual Policy'));
    await userEvent.click(
      await screen.findByRole('option', { name: 'Automatic' }),
    );

    await waitFor(() => {
      expect(accrualStartButton).toBeEnabled();
      expect(accrualStartButton).not.toHaveTextContent('Pick a date');
    });
  });

  test('calls onCancel and submits form values', async () => {
    const onCancel = vi.fn();
    const onSubmit = vi.fn(async () => {});

    render(<ExpenseFormHarness onCancel={onCancel} onSubmit={onSubmit} />);

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(onCancel).toHaveBeenCalledTimes(1);

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        description: 'Internet',
        accountRowId: 'account-1',
      }),
      expect.anything(),
    );
  });
});
