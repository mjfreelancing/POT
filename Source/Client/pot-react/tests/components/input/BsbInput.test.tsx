import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { act } from 'react';
import { describe, expect, it, vi } from 'vitest';

import BsbInput from '@/components/input/BsbInput';

describe('BsbInput', () => {
  /**
   * Creates a wrapper component that provides controlled component behavior for testing.
   * This mirrors the pattern used for other controlled input wrappers so the component
   * can be exercised with real user interactions and its emitted value can be asserted.
   */
  const createControlledBsbInput = (
    initialValue = '',
    onChange = vi.fn(),
    additionalProps = {},
  ) => {
    const Component = () => {
      const [value, setValue] = React.useState(initialValue);

      const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        onChange(event);
        setValue(event.target.value);
      };

      return (
        <BsbInput value={value} onChange={handleChange} {...additionalProps} />
      );
    };

    return Component;
  };

  it('renders empty when no initial value is provided', () => {
    render(<BsbInput />);
    expect(screen.getByRole('textbox')).toHaveValue('');
  });

  it('renders an initial canonical value unchanged', () => {
    render(<BsbInput value="123-456" />);
    expect(screen.getByRole('textbox')).toHaveValue('123-456');
  });

  it('renders an initial raw digit value in canonical format', () => {
    render(<BsbInput value="123456" />);
    expect(screen.getByRole('textbox')).toHaveValue('123-456');
  });

  it('formats digits typed without a dash into canonical format', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledBsbInput('', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '111222');
    });

    expect(input).toHaveValue('111-222');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: '111-222' }),
      }),
    );
  });

  it('formats digits typed with a dash into canonical format', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123-456');
    });

    expect(input).toHaveValue('123-456');
  });

  it('inserts the dash automatically after the third digit', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '1234');
    });

    expect(input).toHaveValue('123-4');
  });

  it('limits input to six digits', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '1234567');
    });

    expect(input).toHaveValue('123-456');
  });

  it('ignores non-numeric characters', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, 'abc');
    });

    expect(input).toHaveValue('');
  });

  it('allows digits through even when non-numeric keys are mixed in', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '1a2b3c');
    });

    expect(input).toHaveValue('123');
  });

  it('removes the last digit on backspace', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123456');
      await user.keyboard('{Backspace}');
    });

    expect(input).toHaveValue('123-45');
  });

  it('normalizes a pasted raw digit value', async () => {
    const onChange = vi.fn();

    const Component = createControlledBsbInput('', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      fireEvent.change(input, { target: { value: '999888' } });
    });

    expect(input).toHaveValue('999-888');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: '999-888' }),
      }),
    );
  });

  it('preserves the editing order when a middle digit is corrected', async () => {
    const user = userEvent.setup();

    const Component = createControlledBsbInput();
    render(<Component />);

    const input = screen.getByRole('textbox') as HTMLInputElement;

    await act(async () => {
      await user.type(input, '123456');
    });

    expect(input).toHaveValue('123-456');

    // Move the caret back three characters (past the last digit) and replace it.
    await act(async () => {
      await user.click(input);
      input.setSelectionRange(6, 7);
      await user.keyboard('{Backspace}');
      await user.type(input, '9');
    });

    expect(input).toHaveValue('123-459');
  });

  it('renders read-only canonical values for edit mode', () => {
    render(<BsbInput value="123-456" readOnly />);
    expect(screen.getByRole('textbox')).toHaveValue('123-456');
    expect(screen.getByRole('textbox')).toHaveAttribute('readonly');
  });
});
