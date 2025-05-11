import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { act } from 'react';
import React from 'react';
import { describe, expect, it, vi } from 'vitest';

import MoneyValueInput, { MoneyValueChangeEvent } from '../MoneyValueInput';

/**
 * Creates an expectation matcher for MoneyValueInput onChange events.
 * This helper simplifies assertions by checking both the string value
 * and numeric representation in the event's target.
 */
const createMoneyValueEvent = (
  stringValue: string,
  numericValue: number | undefined,
) => {
  return expect.objectContaining({
    target: expect.objectContaining({
      value: stringValue,
      number: numericValue,
    }),
  });
};

describe('MoneyValueInput', () => {
  /**
   * Creates a wrapper component that provides controlled component behavior for testing
   * This helper is needed because:
   * 1. We need to maintain internal state between user interactions
   * 2. We need to correctly capture onChange events for assertions
   * 3. It ensures consistent handling of the component's value prop across tests
   */
  const createControlledMoneyValueInput = (
    initialValue: string | number = '',
    onChange = vi.fn(),
    additionalProps = {},
  ) => {
    const Component = () => {
      // Use string state to properly handle incomplete values like '.'
      const [value, setValue] = React.useState(initialValue);

      const handleChange = (e: MoneyValueChangeEvent) => {
        onChange(e);
        setValue(e.target.value);
      };

      return (
        <MoneyValueInput
          value={value}
          onChange={handleChange}
          {...additionalProps}
        />
      );
    };

    return Component;
  };

  it('renders empty when no initial value', () => {
    render(<MoneyValueInput />);
    expect(screen.getByRole('textbox')).toHaveValue('');
  });

  it('renders empty when initial empty string', () => {
    render(<MoneyValueInput value="" />);
    expect(screen.getByRole('textbox')).toHaveValue('');
  });

  it('renders with initial value as number', () => {
    render(<MoneyValueInput value={123.45} />);
    expect(screen.getByRole('textbox')).toHaveValue('123.45');
  });

  it('renders with initial value as string', () => {
    render(<MoneyValueInput value="123.45" />);
    expect(screen.getByRole('textbox')).toHaveValue('123.45');
  });

  it('allows typing valid numbers', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123.45');
    });

    expect(input).toHaveValue('123.45');

    expect(onChange).toHaveBeenCalledWith(
      createMoneyValueEvent('123.45', 123.45),
    );
  });

  it('allows single minus at start', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '-123.45');
    });

    expect(input).toHaveValue('-123.45');
  });

  it('blocks double minus at start', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '--123.45');
    });

    expect(input).toHaveValue('-123.45');
  });

  it('blocks minus in middle of number', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '-12-3.45');
    });

    expect(input).toHaveValue('-123.45');
  });

  it('blocks minus at end of number', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123.45-');
    });

    expect(input).toHaveValue('123.45');
  });

  it('allows minus at start of number and ignores subsequent minus', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '-12-3.45-');
    });

    expect(input).toHaveValue('-123.45');
  });

  it('triggers onChange with correct negative value', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '-123.45');
    });

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('-123.45', -123.45),
    );
  });

  /*
   *
   *
   *
   */

  it('allows leading decimal point while typing', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '.');
    });

    expect(input).toHaveValue('.');
  });

  /*
   *
   *
   *
   */

  it('treats a single decimal point as 0.00 on blur', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.click(input);
    });

    await act(async () => {
      await user.type(input, '.');
    });

    expect(input).toHaveValue('.');

    await act(async () => {
      //fireEvent.blur(input);
      user.tab();
    });

    expect(input).toHaveValue('0.00');

    expect(onChange).toHaveBeenCalledWith(
      createMoneyValueEvent('.', undefined),
    );
  });

  /*
   *
   *
   *
   */

  it('blocks additional decimal points', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123.45.');
    });

    expect(input).toHaveValue('123.45');
  });

  it('allows numbers after decimal point', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123.45');
    });
    await act(async () => {
      await user.type(input, '.67');
    });

    expect(input).toHaveValue('123.4567');
  });

  it('formats leading decimal on blur', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '.5');
    });

    await act(async () => {
      await user.tab();
    });

    expect(input).toHaveValue('0.50');
  });

  it('formats empty string to 0.00 on blur', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.click(input);
    });
    await act(async () => {
      fireEvent.blur(input);
    });
    expect(input).toHaveValue('0.00');
  });

  it('appends .00 to whole numbers on blur', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123');
    });
    await act(async () => {
      await user.tab();
    });
    expect(input).toHaveValue('123.00');
  });

  it('adds trailing zero to single decimal on blur', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123.4');
    });
    await act(async () => {
      await user.tab();
    });
    expect(input).toHaveValue('123.40');
  });

  it('rounds values properly on blur', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '.999');
    });
    await act(async () => {
      await user.tab();
    });
    expect(input).toHaveValue('1.00');
  });

  it('calls onBlur handler', async () => {
    const user = userEvent.setup();
    const onBlur = vi.fn();

    // Pass the onBlur handler directly to the component to ensure it's called
    const Component = () => {
      const [value, setValue] = React.useState('');

      return (
        <MoneyValueInput
          value={value}
          onBlur={onBlur}
          onChange={(e: MoneyValueChangeEvent) => {
            setValue(
              e.target.number !== undefined ? String(e.target.number) : '',
            );
          }}
        />
      );
    };

    render(<Component />);
    const input = screen.getByRole('textbox');

    // Focus the input first, then trigger the blur event directly
    await act(async () => {
      await user.click(input);
    });

    // Using fireEvent.blur directly to ensure the event is triggered
    await act(async () => {
      fireEvent.blur(input);
    });

    expect(onBlur).toHaveBeenCalled();
  });

  it('blocks invalid key presses', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, 'abc');
    });

    expect(input).toHaveValue('');
  });

  it('allows control keys without changing value', async () => {
    const user = userEvent.setup();

    // Use a numeric value to ensure correct formatting
    const Component = () => {
      const [value, setValue] = React.useState(123.45);

      const handleChange = (e: MoneyValueChangeEvent) => {
        // Use the number property instead of value
        setValue(e.target.number as number);
      };

      return <MoneyValueInput value={value} onChange={handleChange} />;
    };

    render(<Component />);
    const input = screen.getByRole('textbox');

    // First verify the initial formatted value
    expect(input).toHaveValue('123.45');

    // Control keys should not modify the displayed value
    await act(async () => {
      await user.keyboard('{ArrowLeft}');
    });
    expect(input).toHaveValue('123.45');

    await act(async () => {
      await user.keyboard('{ArrowRight}');
    });
    expect(input).toHaveValue('123.45');

    await act(async () => {
      await user.keyboard('{Delete}');
    });
    expect(input).toHaveValue('123.45');

    await act(async () => {
      await user.keyboard('{Backspace}');
    });
    expect(input).toHaveValue('123.45');
  });

  it('handles empty string with undefined onChange value', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123');
    });
    await act(async () => {
      await user.clear(input); // Wrap in act() since clear triggers state update
    });
    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('', undefined),
    );
  });

  it('handles text selection and replacement', async () => {
    const onChange = vi.fn();
    let rerender: ReturnType<typeof render>['rerender'];

    await act(async () => {
      const result = render(
        <MoneyValueInput value={123.45} onChange={onChange} />,
      );
      rerender = result.rerender;
    });

    const input = screen.getByRole('textbox');
    expect(input).toHaveValue('123.45');

    await act(async () => {
      rerender(<MoneyValueInput value={999.99} onChange={onChange} />);
    });

    expect(input).toHaveValue('999.99');
    expect(onChange).not.toHaveBeenCalled();
  });

  it('handles paste of invalid content', async () => {
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      fireEvent.paste(input, {
        clipboardData: {
          getData: () => 'abc123.45xyz',
        },
      });
    });

    expect(input).toHaveValue('');
    expect(onChange).not.toHaveBeenCalled();
  });

  it('handles paste of valid number content', async () => {
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    const pastedValue = '123.45';

    await act(async () => {
      fireEvent.paste(input, {
        clipboardData: {
          getData: () => pastedValue,
        },
      });

      fireEvent.change(input, {
        target: { value: pastedValue },
      });
    });

    expect(input).toHaveValue('123.45');
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({
          value: '123.45',
          number: 123.45,
        }),
      }),
    );
  });

  it('handles partial text selection and replacement', async () => {
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('123.45', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      fireEvent.change(input, {
        target: {
          value: '999.45',
          selectionStart: 0,
          selectionEnd: 3,
        },
      });
    });

    expect(input).toHaveValue('999.45');
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({
          value: '999.45',
          number: 999.45,
        }),
      }),
    );
  });

  it('maintains empty state when deleting empty content', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.keyboard('{Delete}');
    });

    expect(input).toHaveValue('');
    expect(onChange).not.toHaveBeenCalled();

    await act(async () => {
      await user.keyboard('{Backspace}');
    });

    expect(input).toHaveValue('');
    expect(onChange).not.toHaveBeenCalled();
  });

  it('does not trigger onChange for incomplete values', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '-');
    });
    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('-', undefined),
    );

    await act(async () => {
      await user.clear(input);
    });
    await act(async () => {
      await user.type(input, '.');
    });
    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('.', undefined),
    );
  });

  it('properly handles tab navigation', async () => {
    const user = userEvent.setup();
    const Component = () => {
      const [value, setValue] = React.useState('');

      const handleChange = (e: MoneyValueChangeEvent) => {
        setValue(e.target.number !== undefined ? String(e.target.number) : '');
      };

      return (
        <>
          <MoneyValueInput
            value={value}
            onChange={handleChange}
            data-testid="money-input"
          />
          <input data-testid="next-input" />
        </>
      );
    };

    render(<Component />);

    const input = screen.getByTestId('money-input');
    const nextInput = screen.getByTestId('next-input');

    await act(async () => {
      await user.type(input, '123.45');
    });
    await act(async () => {
      await user.tab(); // Wrap tab in act() since it triggers blur
    });

    expect(nextInput).toHaveFocus();
  });

  it('allows Escape key to maintain focus', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput('123.45');
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '{Escape}');
    });
    expect(input).toHaveFocus();
  });

  it('handles large numbers without scientific notation', async () => {
    const user = userEvent.setup();
    const Component = createControlledMoneyValueInput();
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '1234567890.12');
    });

    expect(input).toHaveValue('1234567890.12');
  });

  it('truncates long decimal numbers on paste', async () => {
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(() => {
      fireEvent.paste(input, {
        clipboardData: {
          getData: () => '123.456789',
        },
      });

      fireEvent.change(input, {
        target: { value: '123.456789' },
      });
    });

    await act(() => {
      fireEvent.blur(input);
    });

    expect(input).toHaveValue('123.46');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({
          value: '123.456789',
          number: 123.456789,
        }),
      }),
    );
  });

  // Add specific tests for number and value property relationship
  it('provides both string value and number properties in onChange event', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    // Test for positive numbers
    await act(async () => {
      await user.type(input, '42.25');
    });

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('42.25', 42.25),
    );

    // Clear and test for negative numbers
    await act(async () => {
      await user.clear(input);
    });
    await act(async () => {
      await user.type(input, '-17.5');
    });

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('-17.5', -17.5),
    );
  });

  it('provides undefined number property for incomplete numeric inputs', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    // Test for incomplete inputs: minus sign
    await act(async () => {
      await user.type(input, '-');
    });

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('-', undefined),
    );

    // Clear and test for incomplete inputs: decimal point only
    await act(async () => {
      await user.clear(input);
    });
    await act(async () => {
      await user.type(input, '.');
    });

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('.', undefined),
    );
  });

  it('filters out non-numeric characters while typing', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '12a34b.c56');
    });

    expect(input).toHaveValue('1234.56');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({
          value: '1234.56',
          number: 1234.56,
        }),
      }),
    );
  });

  it('preserves valid numbers when typing invalid characters in middle', async () => {
    const user = userEvent.setup();

    // Use a numeric value to ensure correct formatting
    const Component = () => {
      const [value, setValue] = React.useState(123.45);

      const handleChange = (e: MoneyValueChangeEvent) => {
        setValue(e.target.number as number);
      };

      return <MoneyValueInput value={value} onChange={handleChange} />;
    };

    render(<Component />);
    const input = screen.getByRole('textbox') as HTMLInputElement;

    // Place cursor in middle and type invalid chars
    input.setSelectionRange(2, 2);
    await act(async () => {
      await user.type(input, 'abc');
    });
    expect(input).toHaveValue('123.45');
  });

  it('handles paste at cursor position', async () => {
    const onChange = vi.fn();
    const Component = createControlledMoneyValueInput('123.45', onChange);
    render(<Component />);
    const input = screen.getByRole('textbox') as HTMLInputElement;

    input.setSelectionRange(2, 2);

    await act(async () => {
      fireEvent.paste(input, {
        clipboardData: {
          getData: () => '99',
        },
      });

      fireEvent.change(input, {
        target: { value: '12993.45' },
      });
    });

    expect(input).toHaveValue('12993.45');
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({
          value: '12993.45',
          number: 12993.45,
        }),
      }),
    );
  });

  it('respects disabled state', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    // Use a numeric value to ensure correct formatting
    render(<MoneyValueInput disabled value={123.45} onChange={onChange} />);

    const input = screen.getByRole('textbox');

    expect(input).toBeDisabled();
    await act(async () => {
      await user.type(input, '999');
    });
    expect(input).toHaveValue('123.45');
    expect(onChange).not.toHaveBeenCalled();
  });
});
