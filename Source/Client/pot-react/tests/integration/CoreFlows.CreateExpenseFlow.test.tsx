import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetAllAccounts } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import CreateExpenseSheet from '@/features/expenses/create/CreateExpenseSheet';
import useCreateExpense from '@/features/expenses/create/hooks/useCreateExpense';
import { SuccessResult, todayIsoFormat } from '@/lib';

import { createAccountWithIdentity } from '../shared/factories/accountFactory';

const createExpenseMock = vi.fn();
const setErrorMock = vi.fn();

vi.mock('@/api/hooks', () => ({
  useApiGetAllAccounts: vi.fn(),
}));

vi.mock('@/features/expenses/create/hooks/useCreateExpense', () => ({
  default: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

function renderCreateExpenseFlow(initialPath: string = '/expenses/create') {
  const queryClient = new QueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <Routes>
          <Route path="/expenses/create" element={<CreateExpenseSheet />} />
          <Route path="/expenses" element={<h1>Expenses page</h1>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('Core Flow Integration - Create Expense', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult([
        createAccountWithIdentity('account-1', 'Main account'),
      ]),
      isLoading: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    vi.mocked(useCreateExpense).mockReturnValue({
      createExpense: createExpenseMock,
    });

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  test('submits a valid expense with account selection and returns to expenses page', async () => {
    // Step 1: Mock a successful expense create response.
    createExpenseMock.mockResolvedValueOnce(
      new SuccessResult({
        rowId: 'expense-created-1',
      }),
    );

    // Step 2: Render the real create-expense route and form.
    renderCreateExpenseFlow('/expenses/create');

    // Step 3: Fill required fields and choose an associated account.
    await userEvent.type(screen.getByLabelText('Description'), 'Internet Bill');
    await userEvent.clear(screen.getByLabelText('Amount'));
    await userEvent.type(screen.getByLabelText('Amount'), '120');

    await userEvent.click(screen.getByLabelText('Associated Account'));
    await userEvent.click(
      await screen.findByRole('option', { name: 'Main account' }),
    );

    // Step 4: Submit and verify expense payload composition.
    await userEvent.click(screen.getByRole('button', { name: 'Create' }));

    expect(createExpenseMock).toHaveBeenCalledWith(
      expect.objectContaining({
        description: 'Internet Bill',
        amount: 120,
        accountRowId: 'account-1',
        nextDue: todayIsoFormat(),
        accrualStart: todayIsoFormat(),
        endDate: null,
      }),
    );

    // Step 5: Verify successful completion returns to expenses route.
    expect(
      await screen.findByRole('heading', { name: 'Expenses page' }),
    ).toBeInTheDocument();
    expect(setErrorMock).not.toHaveBeenCalled();
  });
});
