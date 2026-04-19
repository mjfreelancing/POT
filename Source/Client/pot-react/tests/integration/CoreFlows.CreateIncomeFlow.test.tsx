import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useApiGetAllAccounts } from '@/api/hooks';
import { useErrorContext } from '@/contexts';
import CreateIncomeSheet from '@/features/incomes/create/CreateIncomeSheet';
import useCreateIncome from '@/features/incomes/create/hooks/useCreateIncome';
import { SuccessResult, todayIsoFormat } from '@/lib';

import { createAccountWithIdentity } from '../shared/factories/accountFactory';

const createIncomeMock = vi.fn();
const setErrorMock = vi.fn();

vi.mock('@/api/hooks', () => ({
  useApiGetAllAccounts: vi.fn(),
}));

vi.mock('@/features/incomes/create/hooks/useCreateIncome', () => ({
  default: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

function renderCreateIncomeFlow(initialPath: string = '/incomes/create') {
  const queryClient = new QueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <Routes>
          <Route path="/incomes/create" element={<CreateIncomeSheet />} />
          <Route path="/incomes" element={<h1>Incomes page</h1>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('Core Flow Integration - Create Income', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useApiGetAllAccounts).mockReturnValue({
      data: new SuccessResult([
        createAccountWithIdentity('account-1', 'Main account'),
      ]),
      isLoading: false,
    } as unknown as ReturnType<typeof useApiGetAllAccounts>);

    vi.mocked(useCreateIncome).mockReturnValue({
      createIncome: createIncomeMock,
    });

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  test('submits a valid income with account selection and returns to incomes page', async () => {
    // Step 1: Mock a successful income create response.
    createIncomeMock.mockResolvedValueOnce(
      new SuccessResult({
        rowId: 'income-created-1',
      }),
    );

    // Step 2: Render the real create-income route and form.
    renderCreateIncomeFlow('/incomes/create');

    // Step 3: Fill required fields and choose an associated account.
    await userEvent.type(screen.getByLabelText('Description'), 'Salary');
    await userEvent.clear(screen.getByLabelText('Amount'));
    await userEvent.type(screen.getByLabelText('Amount'), '4200');

    await userEvent.click(screen.getByLabelText('Associated Account'));
    await userEvent.click(
      await screen.findByRole('option', { name: 'Main account' }),
    );

    // Step 4: Submit and verify income payload composition.
    await userEvent.click(screen.getByRole('button', { name: 'Create' }));

    expect(createIncomeMock).toHaveBeenCalledWith(
      expect.objectContaining({
        description: 'Salary',
        amount: 4200,
        accountRowId: 'account-1',
        nextDue: todayIsoFormat(),
        endDate: null,
      }),
    );

    // Step 5: Verify successful completion returns to incomes route.
    expect(
      await screen.findByRole('heading', { name: 'Incomes page' }),
    ).toBeInTheDocument();
    expect(setErrorMock).not.toHaveBeenCalled();
  });
});
