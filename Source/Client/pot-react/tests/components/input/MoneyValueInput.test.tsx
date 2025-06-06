import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { act } from 'react';
import React from 'react';
import { describe, expect, it, vi } from 'vitest';

import MoneyValueInput, {
  MoneyValueChangeEvent,
} from '@/components/input/MoneyValueInput';

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

  it('treats a single decimal point as 0.00 on tab', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox') as HTMLInputElement;

    await act(async () => {
      await user.type(input, '.');
    });

    expect(input).toHaveValue('.');

    await act(async () => {
      await user.tab();
    });

    expect(input).toHaveValue('0.00');
    expect(onChange).toHaveBeenCalledWith(createMoneyValueEvent('0.00', 0.0));
  });

  it('treats a single minus as 0.00 on tab', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput('', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox') as HTMLInputElement;

    await act(async () => {
      await user.type(input, '-');
    });

    expect(input).toHaveValue('-');

    await act(async () => {
      await user.tab();
    });

    expect(input).toHaveValue('0.00');
    expect(onChange).toHaveBeenCalledWith(createMoneyValueEvent('0.00', 0.0));
  });

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

  it('formats leading decimal on blur', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '.5');
      await user.tab();
    });

    expect(input).toHaveValue('0.50');
  });

  it('formats empty string to 0.00 on tab', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.click(input);
    });

    await act(async () => {
      await user.tab();
    });

    expect(input).toHaveValue('0.00');
  });

  it('appends .00 to whole numbers on tab', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '123');
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
      await user.tab();
    });

    expect(input).toHaveValue('123.40');
  });

  it('rounds values properly on tab', async () => {
    const user = userEvent.setup();

    const Component = createControlledMoneyValueInput();
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.type(input, '.999');
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

    // Focus the input first, then trigger fireEvent.blur directly to ensure the event is triggered
    await act(async () => {
      await user.click(input);
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
      await user.clear(input);
    });

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('', undefined),
    );
  });

  it('updates displayed value on value prop replacement without firing onChange', async () => {
    const onChange = vi.fn();
    let rerender: ReturnType<typeof render>['rerender'];

    // This test simulates replacing the entire input text by updating the `value` prop:
    // 1. Render with initial value prop (123.45).
    // 2. Rerender with a new value prop (999.99), simulating a full text replacement.
    // 3. Verify the input shows the new prop value and that no onChange event is emitted.
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
      createMoneyValueEvent('123.45', 123.45),
    );
  });

  it('handles partial text selection and replacement', async () => {
    const onChange = vi.fn();

    // Initial value: '123.45'
    const Component = createControlledMoneyValueInput('123.45', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      // Simulate a change event where '23.4' (from '1[23.4]5') is conceptually replaced by '88.888',
      // with the input field reflecting this as '188.885'.
      // The selectionStart/End here describe the part of the original string
      // that was selected for replacement to achieve the new 'value'.
      fireEvent.change(input, {
        target: {
          value: '188.885', // The new value of the input field
          selectionStart: 1, // Corresponds to the start of '23.4' in '123.45'
          selectionEnd: 5, // Corresponds to the end of '23.4' in '123.45'
        },
      });
    });

    // Assert the input field displays the new value
    expect(input).toHaveValue('188.885');

    // Assert onChange was called with the correct new string and numeric value
    expect(onChange).toHaveBeenCalledWith(
      createMoneyValueEvent('188.885', 188.885),
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

  it('onChange reports undefined number for incomplete values', async () => {
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
      await user.tab();
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

    await act(async () => {
      fireEvent.paste(input, {
        clipboardData: {
          getData: () => '123.456789',
        },
      });

      fireEvent.change(input, {
        target: { value: '123.456789' },
      });
    });

    await act(async () => {
      fireEvent.blur(input);
    });

    expect(input).toHaveValue('123.46');

    expect(onChange).toHaveBeenLastCalledWith(
      createMoneyValueEvent('123.456789', 123.456789),
    );
  });

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
      createMoneyValueEvent('1234.56', 1234.56),
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
      createMoneyValueEvent('12993.45', 12993.45),
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

  it('formats empty input to 0.00 when deleting all text and tabbing out', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput('123.45', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox');

    await act(async () => {
      await user.clear(input);
    });

    expect(input).toHaveValue('');

    await act(async () => {
      await user.tab();
    });

    expect(input).toHaveValue('0.00');
    expect(onChange).toHaveBeenCalledWith(createMoneyValueEvent('0.00', 0));
  });

  it('formats single decimal point to 0.00 when replacing content and tabbing out', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const Component = createControlledMoneyValueInput('123.45', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox') as HTMLInputElement;

    expect(input).toHaveValue('123.45');

    await act(async () => {
      await user.click(input);
      input.select();
      await user.keyboard('.');
    });

    expect(input).toHaveValue('.');

    await act(async () => {
      await user.tab();
    });

    expect(input).toHaveValue('0.00');
    expect(onChange).toHaveBeenCalledWith(createMoneyValueEvent('0.00', 0));
  });

  it('formats just minus sign to 0.00 when replacing content and tabbing out', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    // Use the existing helper to create a controlled component
    const Component = createControlledMoneyValueInput('123.45', onChange);
    render(<Component />);

    const input = screen.getByRole('textbox') as HTMLInputElement;

    // Verify initial value
    expect(input).toHaveValue('123.45');

    // Simulate a real user: select all text and type minus
    await act(async () => {
      await user.click(input);
      input.select();

      // Clear the onChange mock to focus on what happens after typing
      onChange.mockClear();

      // Type minus to replace selected text
      await user.keyboard('-');
    });

    // Verify the input now has just the minus sign
    expect(input).toHaveValue('-');

    // Tab out - this is what triggers the formatting in real use
    await act(async () => {
      await user.tab();
    });

    // Verify the value is correctly formatted
    expect(input).toHaveValue('0.00');

    // Verify the right onChange event was fired
    expect(onChange).toHaveBeenCalledWith(createMoneyValueEvent('0.00', 0));
  });
});
