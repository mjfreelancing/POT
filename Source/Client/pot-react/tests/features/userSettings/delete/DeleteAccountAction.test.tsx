import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { BadRequestError } from '@/api/errors/apiErrors';
import { useAuthContext } from '@/features/auth/contexts';
import useClearSession from '@/features/auth/hooks/useClearSession';
import DeleteAccountAction from '@/features/userSettings/delete/DeleteAccountAction';
import useDeleteOwnAccount from '@/features/userSettings/delete/hooks/useDeleteOwnAccount';
import { FailResult, SuccessResult } from '@/lib';

const navigateMock = vi.fn();
const clearSessionMock = vi.fn();
const deleteOwnAccountMock = vi.fn();
const setSessionNoticeMock = vi.fn();

vi.mock('react-router', async importOriginal => {
  const actual = await importOriginal<typeof import('react-router')>();

  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock('@/features/auth/contexts', () => ({
  useAuthContext: vi.fn(),
}));

vi.mock('@/features/auth/hooks/useClearSession', () => ({
  default: vi.fn(),
}));

vi.mock('@/features/auth/sessionNotice', () => ({
  setSessionNotice: (...args: unknown[]) => setSessionNoticeMock(...args),
}));

vi.mock('@/features/userSettings/delete/hooks/useDeleteOwnAccount', () => ({
  default: vi.fn(),
}));

function renderAction() {
  return render(<DeleteAccountAction />);
}

async function openConfirmation() {
  await userEvent.click(
    screen.getByRole('button', { name: 'Delete my account' }),
  );

  return screen.getByRole('alertdialog');
}

describe('DeleteAccountAction', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuthContext).mockReturnValue({
      userInfo: { username: 'maria' },
    } as ReturnType<typeof useAuthContext>);

    vi.mocked(useClearSession).mockReturnValue(clearSessionMock);
    vi.mocked(useDeleteOwnAccount).mockReturnValue({
      deleteOwnAccount: deleteOwnAccountMock,
    });
  });

  test('offers the action with only the permanence warning', () => {
    renderAction();

    expect(
      screen.getByRole('button', { name: 'Delete my account' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('This permanently deletes your account and cannot be undone.'),
    ).toBeInTheDocument();
  });

  test('enables the confirmation only once the username and password are typed', async () => {
    renderAction();

    await openConfirmation();

    const confirmButton = screen.getByRole('button', { name: 'Delete Account' });

    expect(confirmButton).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Username'), 'maria');

    expect(confirmButton).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Your password'), 'secret');

    expect(confirmButton).toBeEnabled();
  });

  test('deletes the account, clears the session and returns to the login page', async () => {
    deleteOwnAccountMock.mockResolvedValue(new SuccessResult(undefined));

    renderAction();

    await openConfirmation();
    await userEvent.type(screen.getByLabelText('Username'), 'maria');
    await userEvent.type(screen.getByLabelText('Your password'), 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Delete Account' }));

    expect(deleteOwnAccountMock).toHaveBeenCalledWith({
      username: 'maria',
      password: 'secret',
    });
    expect(setSessionNoticeMock).toHaveBeenCalledWith(
      'Your account has been deleted.',
    );
    expect(clearSessionMock).toHaveBeenCalledTimes(1);
    expect(navigateMock).toHaveBeenCalledWith('/login', { replace: true });
  });

  test('keeps the confirmation open with the refusal and stays signed in', async () => {
    deleteOwnAccountMock.mockResolvedValue(
      new FailResult(new BadRequestError('The password is incorrect.')),
    );

    renderAction();

    const dialog = await openConfirmation();
    await userEvent.type(screen.getByLabelText('Username'), 'maria');
    await userEvent.type(screen.getByLabelText('Your password'), 'wrong');
    await userEvent.click(screen.getByRole('button', { name: 'Delete Account' }));

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'The password is incorrect.',
    );
    expect(dialog).toBeInTheDocument();
    expect(clearSessionMock).not.toHaveBeenCalled();
    expect(navigateMock).not.toHaveBeenCalled();
  });

  test('clears a previous refusal when the confirmation is reopened', async () => {
    deleteOwnAccountMock.mockResolvedValue(
      new FailResult(new BadRequestError('The password is incorrect.')),
    );

    renderAction();

    await openConfirmation();
    await userEvent.type(screen.getByLabelText('Username'), 'maria');
    await userEvent.type(screen.getByLabelText('Your password'), 'wrong');
    await userEvent.click(screen.getByRole('button', { name: 'Delete Account' }));

    expect(await screen.findByRole('alert')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));
    await openConfirmation();

    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});
