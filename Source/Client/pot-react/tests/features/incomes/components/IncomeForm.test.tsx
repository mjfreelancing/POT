import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useForm } from 'react-hook-form';
import { describe, expect, test, vi } from 'vitest';

import IncomeForm from '@/features/incomes/components/IncomeForm';
import type { IncomeFormData } from '@/features/incomes/schemas/incomeFormSchema';
import { Frequency } from '@/lib';

import { createAccountWithIdentity } from '../../../shared/factories/accountFactory';

type IncomeHarnessProps = {
  isEditMode?: boolean;
  onSubmit?: (values: IncomeFormData) => Promise<void>;
  onCancel?: () => void;
  defaultValues?: Partial<IncomeFormData>;
};

function IncomeFormHarness({
  isEditMode = false,
  onSubmit = async () => {},
  onCancel = vi.fn(),
  defaultValues,
}: IncomeHarnessProps) {
  const form = useForm<IncomeFormData>({
    defaultValues: {
      excludeFromCalcs: false,
      description: 'Salary',
      nextDue: '2026-01-10',
      endDate: '2026-02-10',
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 4200,
      note: '',
      accountRowId: 'account-1',
      ...defaultValues,
    },
  });

  return (
    <IncomeForm
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

describe('IncomeForm', () => {
  test('clears and disables end date and normalizes frequencyCount when selecting One Time, then re-enables and restores recurring count', async () => {
    render(<IncomeFormHarness />);

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

  test('normalizes recurring frequencyCount from 0 to 1 on initial render', async () => {
    render(
      <IncomeFormHarness
        defaultValues={{
          frequency: Frequency.Months,
          frequencyCount: 0,
          endDate: undefined,
        }}
      />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText('Every')).toHaveValue(1);
    });
  });

  test('sets focus to amount input in edit mode', async () => {
    render(<IncomeFormHarness isEditMode={true} />);

    await waitFor(() => {
      expect(screen.getByLabelText('Amount')).toHaveFocus();
    });
  });

  test('clears end date when Clear action is clicked', async () => {
    render(<IncomeFormHarness />);

    const endDateButton = document.getElementById(
      'endDate-picker',
    ) as HTMLButtonElement;

    await userEvent.click(screen.getByRole('button', { name: 'Clear' }));

    await waitFor(() => {
      expect(endDateButton).toBeEnabled();
      expect(endDateButton).toHaveTextContent('Pick a date');
    });
  });

  test('calls onCancel and submits form values', async () => {
    const onCancel = vi.fn();
    const onSubmit = vi.fn(async () => {});

    render(<IncomeFormHarness onCancel={onCancel} onSubmit={onSubmit} />);

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(onCancel).toHaveBeenCalledTimes(1);

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        description: 'Salary',
        accountRowId: 'account-1',
      }),
      expect.anything(),
    );
  });
});
