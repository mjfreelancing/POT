import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';

import DestructiveConfirmDialog from '@/components/dialog/destructiveConfirmDialog';

function renderDialog(
  overrides: Partial<React.ComponentProps<typeof DestructiveConfirmDialog>> = {},
) {
  const onConfirm = vi.fn();
  const onCancel = vi.fn();

  render(
    <DestructiveConfirmDialog
      open
      title="Delete Maria"
      description={<p>This cannot be undone.</p>}
      confirmLabel="Delete User"
      fields={[
        {
          id: 'confirm-username',
          label: 'Username',
          expectedValue: 'maria',
        },
      ]}
      passwordLabel="Your password"
      onConfirm={onConfirm}
      onCancel={onCancel}
      {...overrides}
    />,
  );

  return { onConfirm, onCancel };
}

function getDeleteButton() {
  return screen.getByRole('button', { name: 'Delete User' });
}

describe('DestructiveConfirmDialog', () => {
  test('keeps the destructive action disabled until the username matches', async () => {
    renderDialog();

    expect(getDeleteButton()).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Username'), 'not-maria');

    expect(getDeleteButton()).toBeDisabled();

    await userEvent.clear(screen.getByLabelText('Username'));
    await userEvent.type(screen.getByLabelText('Username'), 'maria');

    expect(getDeleteButton()).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Your password'), 'secret');

    expect(getDeleteButton()).toBeEnabled();
  });

  test('matches the typed value trimmed and case-insensitively', async () => {
    renderDialog();

    await userEvent.type(screen.getByLabelText('Username'), '  MARIA  ');
    await userEvent.type(screen.getByLabelText('Your password'), 'secret');

    expect(getDeleteButton()).toBeEnabled();
  });

  test('requires the password before the destructive action is enabled', async () => {
    renderDialog();

    await userEvent.type(screen.getByLabelText('Username'), 'maria');

    expect(getDeleteButton()).toBeDisabled();
  });

  test('reports the entered values on confirm', async () => {
    const { onConfirm } = renderDialog();

    await userEvent.type(screen.getByLabelText('Username'), 'maria');
    await userEvent.type(screen.getByLabelText('Your password'), 'secret');
    await userEvent.click(getDeleteButton());

    expect(onConfirm).toHaveBeenCalledWith({
      fields: { 'confirm-username': 'maria' },
      password: 'secret',
    });
  });

  test('focuses Cancel by default', () => {
    renderDialog();

    expect(screen.getByRole('button', { name: 'Cancel' })).toHaveFocus();
  });

  test('disables both actions while busy and does not report a confirm', async () => {
    const { onConfirm } = renderDialog({ busy: true });

    await userEvent.type(screen.getByLabelText('Username'), 'maria');
    await userEvent.type(screen.getByLabelText('Your password'), 'secret');

    expect(getDeleteButton()).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
    expect(onConfirm).not.toHaveBeenCalled();
  });

  test('cannot be dismissed by clicking outside it', () => {
    const { onCancel } = renderDialog();

    fireEvent.pointerDown(document.body);
    fireEvent.click(document.body);

    expect(
      screen.getByRole('heading', { name: 'Delete Maria' }),
    ).toBeInTheDocument();
    expect(onCancel).not.toHaveBeenCalled();
  });

  test('reports cancel', async () => {
    const { onCancel } = renderDialog();

    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(onCancel).toHaveBeenCalled();
  });
});
