import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { useErrorContext } from '@/contexts';
import CreateAccountSheet from '@/features/accounts/create/CreateAccountSheet';
import useCreateAccount from '@/features/accounts/create/hooks/useCreateAccount';
import { SuccessResult } from '@/lib';

const createAccountMock = vi.fn();
const setErrorMock = vi.fn();

vi.mock('@/features/accounts/create/hooks/useCreateAccount', () => ({
  default: vi.fn(),
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

function renderCreateAccountFlow(initialPath = '/accounts/create') {
  const queryClient = new QueryClient();

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <Routes>
          <Route path="/accounts/create" element={<CreateAccountSheet />} />
          <Route path="/accounts" element={<h1>Accounts page</h1>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('Core Flow Integration - Create Account', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useCreateAccount).mockReturnValue({
      createAccount: createAccountMock,
    });

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  test('submits a valid account and returns to accounts page', async () => {
    // Step 1: Mock a successful account create response.
    createAccountMock.mockResolvedValueOnce(
      new SuccessResult({
        rowId: 'account-created-1',
      }),
    );

    // Step 2: Render the real create-account route and sheet UI.
    renderCreateAccountFlow('/accounts/create');

    // Step 3: Fill the create-account form with valid values.
    await userEvent.type(screen.getByLabelText('BSB'), '123-456');
    await userEvent.type(screen.getByLabelText('Account Number'), '12345678');
    await userEvent.type(
      screen.getByLabelText('Description'),
      'Integration Account',
    );
    await userEvent.clear(screen.getByLabelText('Balance'));
    await userEvent.type(screen.getByLabelText('Balance'), '1500');
    await userEvent.clear(screen.getByLabelText('Reserved'));
    await userEvent.type(screen.getByLabelText('Reserved'), '250');

    // Step 4: Submit and verify payload sent to create hook.
    await userEvent.click(screen.getByRole('button', { name: 'Create' }));

    expect(createAccountMock).toHaveBeenCalledWith({
      bsb: '123-456',
      number: '12345678',
      description: 'Integration Account',
      balance: 1500,
      reserved: 250,
    });

    // Step 5: Verify successful completion navigates back to accounts route.
    expect(
      await screen.findByRole('heading', { name: 'Accounts page' }),
    ).toBeInTheDocument();
    expect(setErrorMock).not.toHaveBeenCalled();
  });

  test('masks raw BSB digits to the canonical format before submit', async () => {
    // Step 1: Mock a successful account create response.
    createAccountMock.mockResolvedValueOnce(
      new SuccessResult({
        rowId: 'account-created-2',
      }),
    );

    // Step 2: Render the real create-account route and sheet UI.
    renderCreateAccountFlow('/accounts/create');

    // Step 3: Type the BSB without a dash; the mask should normalize it to XXX-XXX.
    await userEvent.type(screen.getByLabelText('BSB'), '111222');
    await userEvent.type(screen.getByLabelText('Account Number'), '12345678');
    await userEvent.type(
      screen.getByLabelText('Description'),
      'Masked BSB Account',
    );
    await userEvent.clear(screen.getByLabelText('Balance'));
    await userEvent.type(screen.getByLabelText('Balance'), '100');
    await userEvent.clear(screen.getByLabelText('Reserved'));
    await userEvent.type(screen.getByLabelText('Reserved'), '10');

    // Step 4: Submit and verify the payload contains the canonical BSB format.
    await userEvent.click(screen.getByRole('button', { name: 'Create' }));

    expect(createAccountMock).toHaveBeenCalledWith({
      bsb: '111-222',
      number: '12345678',
      description: 'Masked BSB Account',
      balance: 100,
      reserved: 10,
    });
  });
});
