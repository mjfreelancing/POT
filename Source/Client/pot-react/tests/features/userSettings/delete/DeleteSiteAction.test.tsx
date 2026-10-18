import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import { BadRequestError } from '@/api/errors/apiErrors';
import { useAuthContext } from '@/features/auth/contexts';
import useClearSession from '@/features/auth/hooks/useClearSession';
import DeleteSiteAction from '@/features/userSettings/delete/DeleteSiteAction';
import useDeleteCurrentSite from '@/features/userSettings/delete/hooks/useDeleteCurrentSite';
import { FailResult, SuccessResult } from '@/lib';

const navigateMock = vi.fn();
const clearSessionMock = vi.fn();
const deleteCurrentSiteMock = vi.fn();
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

vi.mock('@/features/userSettings/delete/hooks/useDeleteCurrentSite', () => ({
  default: vi.fn(),
}));

function renderAction() {
  return render(<DeleteSiteAction />);
}

async function openConfirmation() {
  await userEvent.click(
    screen.getByRole('button', { name: 'Delete this site' }),
  );

  return screen.getByRole('alertdialog');
}

async function fillConfirmation(
  siteName: string,
  username: string,
  password: string,
) {
  await userEvent.type(screen.getByLabelText('Site name'), siteName);
  await userEvent.type(screen.getByLabelText('Username'), username);
  await userEvent.type(screen.getByLabelText('Your password'), password);
}

describe('DeleteSiteAction', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useAuthContext).mockReturnValue({
      userInfo: {
        username: 'maria',
        site: { name: 'Site A' },
      },
    } as ReturnType<typeof useAuthContext>);

    vi.mocked(useClearSession).mockReturnValue(clearSessionMock);
    vi.mocked(useDeleteCurrentSite).mockReturnValue({
      deleteCurrentSite: deleteCurrentSiteMock,
    });
  });

  test('offers the action with the scope of what is destroyed', () => {
    renderAction();

    expect(
      screen.getByRole('button', { name: 'Delete this site' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'This permanently deletes every user, account, expense and income in this site.',
      ),
    ).toBeInTheDocument();
  });

  test('names the site in the confirmation title', async () => {
    renderAction();

    await openConfirmation();

    expect(
      screen.getByRole('heading', { name: 'Delete Site A' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Site name')).toHaveAttribute(
      'placeholder',
      'Site A',
    );
  });

  test('enables the confirmation only once the site name, username and password all match', async () => {
    renderAction();

    await openConfirmation();

    const confirmButton = screen.getByRole('button', { name: 'Delete Site' });

    expect(confirmButton).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Site name'), 'Site A');
    await userEvent.type(screen.getByLabelText('Username'), 'maria');

    expect(confirmButton).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Your password'), 'secret');

    expect(confirmButton).toBeEnabled();
  });

  test('stays disabled when the site name or username is wrong', async () => {
    renderAction();

    await openConfirmation();
    await fillConfirmation('Another Site', 'maria', 'secret');

    expect(screen.getByRole('button', { name: 'Delete Site' })).toBeDisabled();

    await userEvent.clear(screen.getByLabelText('Site name'));
    await userEvent.clear(screen.getByLabelText('Username'));
    await userEvent.type(screen.getByLabelText('Site name'), 'Site A');
    await userEvent.type(screen.getByLabelText('Username'), 'someone-else');

    expect(screen.getByRole('button', { name: 'Delete Site' })).toBeDisabled();
  });

  test('deletes the site, clears the session and returns to the login page', async () => {
    deleteCurrentSiteMock.mockResolvedValue(new SuccessResult(undefined));

    renderAction();

    await openConfirmation();
    await fillConfirmation('Site A', 'maria', 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Delete Site' }));

    expect(deleteCurrentSiteMock).toHaveBeenCalledWith({
      siteName: 'Site A',
      username: 'maria',
      password: 'secret',
    });
    expect(setSessionNoticeMock).toHaveBeenCalledWith(
      'Your site has been deleted.',
    );
    expect(clearSessionMock).toHaveBeenCalledTimes(1);
    expect(navigateMock).toHaveBeenCalledWith('/login', { replace: true });
  });

  test('keeps the confirmation open with a refusal and stays signed in', async () => {
    deleteCurrentSiteMock.mockResolvedValue(
      new FailResult(
        new BadRequestError(
          'This site contains the last enabled platform administrator.',
        ),
      ),
    );

    renderAction();

    const dialog = await openConfirmation();
    await fillConfirmation('Site A', 'maria', 'secret');
    await userEvent.click(screen.getByRole('button', { name: 'Delete Site' }));

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'This site contains the last enabled platform administrator.',
    );
    expect(dialog).toBeInTheDocument();
    expect(clearSessionMock).not.toHaveBeenCalled();
    expect(navigateMock).not.toHaveBeenCalled();
  });
});
