import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import MoneyValueInput from '../MoneyValueInput';

describe('MoneyValueInput', () => {
  it('renders with initial value', () => {
    render(<MoneyValueInput value={123.45} />);
    expect(screen.getByRole('textbox')).toHaveValue('123.45');
  });

  it('renders empty when no initial value', () => {
    render(<MoneyValueInput />);
    expect(screen.getByRole('textbox')).toHaveValue('');
  });

  it('allows typing valid numbers', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    await user.type(input, '123.45');
    expect(input).toHaveValue('123.45');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: 123.45 }),
      }),
    );
  });

  it('allows single minus at start', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '-123.45');
    expect(input).toHaveValue('-123.45');
  });

  it('blocks double minus at start', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '--123.45');
    expect(input).toHaveValue('-123.45');
  });

  it('blocks minus in middle of number', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '-12-3.45');
    expect(input).toHaveValue('-123.45');
  });

  it('blocks minus at end of number', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '123.45-');
    expect(input).toHaveValue('123.45');
  });

  it('triggers onChange with correct negative value', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    await user.type(input, '-123.45');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: -123.45 }),
      }),
    );
  });

  it('blocks additional decimal points', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '123.45.');
    expect(input).toHaveValue('123.45');
  });

  it('allows numbers after decimal point', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '123.45');
    await user.type(input, '.67');
    expect(input).toHaveValue('123.4567');
  });

  it('allows leading decimal point while typing', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '.');
    expect(input).toHaveValue('.');
  });

  it('formats leading decimal on blur', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '.5');
    await user.tab();
    expect(input).toHaveValue('0.50');
  });

  it('handles number formatting on blur', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    // Empty string to 0.00
    await user.click(input);
    fireEvent.blur(input);
    expect(input).toHaveValue('0.00');

    // Whole number adds .00
    await user.clear(input);
    await user.type(input, '123');
    await user.tab();
    expect(input).toHaveValue('123.00');

    // Single decimal adds trailing zero
    await user.clear(input);
    await user.type(input, '123.4');
    await user.tab();
    expect(input).toHaveValue('123.40');

    // Round up .999
    await user.clear(input);
    await user.type(input, '.999');
    await user.tab();
    expect(input).toHaveValue('1.00');
  });

  it('calls onBlur handler', async () => {
    const user = userEvent.setup();
    const onBlur = vi.fn();
    render(<MoneyValueInput onBlur={onBlur} />);
    const input = screen.getByRole('textbox');

    await user.click(input);
    await user.tab();
    expect(onBlur).toHaveBeenCalled();
  });

  it('blocks invalid key presses', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, 'abc');
    expect(input).toHaveValue('');
  });

  it('allows control keys without changing value', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput value={123.45} />);
    const input = screen.getByRole('textbox');

    // First verify the initial formatted value
    expect(input).toHaveValue('123.45');

    // Control keys should not modify the displayed value
    await user.keyboard('{ArrowLeft}');
    expect(input).toHaveValue('123.45');

    await user.keyboard('{ArrowRight}');
    expect(input).toHaveValue('123.45');

    await user.keyboard('{Delete}');
    expect(input).toHaveValue('123.45');

    await user.keyboard('{Backspace}');
    expect(input).toHaveValue('123.45');
  });

  it('handles empty string with undefined onChange value', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    await user.type(input, '123');
    await user.clear(input);
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: undefined }),
      }),
    );
  });

  it('handles text selection and replacement', async () => {
    const onChange = vi.fn();
    const { rerender } = render(
      <MoneyValueInput value={123.45} onChange={onChange} />,
    );

    // Verify initial value
    const input = screen.getByRole('textbox');
    expect(input).toHaveValue('123.45');

    // Simulate parent component updating the value
    rerender(<MoneyValueInput value={999.99} onChange={onChange} />);

    expect(input).toHaveValue('999.99');
    expect(onChange).not.toHaveBeenCalled();
  });

  it('handles paste of invalid content', async () => {
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    // Simulate paste event
    fireEvent.paste(input, {
      clipboardData: {
        getData: () => 'abc123.45xyz',
      },
    });

    // No change event should occur for invalid content
    expect(input).toHaveValue('');
    expect(onChange).not.toHaveBeenCalled();
  });

  it('handles paste of valid number content', async () => {
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    const pastedValue = '123.45';

    // Simulate paste event
    fireEvent.paste(input, {
      clipboardData: {
        getData: () => pastedValue,
      },
    });

    // Simulate the change event that would follow the paste
    fireEvent.change(input, {
      target: { value: pastedValue },
    });

    expect(input).toHaveValue('123.45');
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: 123.45 }),
      }),
    );
  });

  it('handles partial text selection and replacement', async () => {
    const onChange = vi.fn();
    render(<MoneyValueInput value={123.45} onChange={onChange} />);
    const input = screen.getByRole('textbox');

    // Simulate selection and input change directly
    fireEvent.change(input, {
      target: {
        value: '999.45',
        selectionStart: 0,
        selectionEnd: 3,
      },
    });

    expect(input).toHaveValue('999.45');
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: 999.45 }),
      }),
    );
  });

  it('maintains empty state when deleting empty content', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    await user.keyboard('{Delete}');
    expect(input).toHaveValue('');
    expect(onChange).not.toHaveBeenCalled();

    await user.keyboard('{Backspace}');
    expect(input).toHaveValue('');
    expect(onChange).not.toHaveBeenCalled();
  });

  it('does not trigger onChange for incomplete values', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    // Test various incomplete values
    await user.type(input, '-');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: undefined }),
      }),
    );

    await user.clear(input);
    await user.type(input, '.');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: undefined }),
      }),
    );
  });

  it('properly handles tab navigation', async () => {
    const user = userEvent.setup();
    render(
      <>
        <MoneyValueInput data-testid="money-input" />
        <input data-testid="next-input" />
      </>,
    );

    const input = screen.getByTestId('money-input');
    const nextInput = screen.getByTestId('next-input');

    await user.type(input, '123.45');
    await user.tab();

    expect(nextInput).toHaveFocus();
  });

  it('allows Escape key to maintain focus', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput value={123.45} />);
    const input = screen.getByRole('textbox');

    await user.type(input, '{Escape}');
    expect(input).toHaveFocus();
  });

  it('handles large numbers without scientific notation', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput />);
    const input = screen.getByRole('textbox');

    await user.type(input, '1234567890.12');
    expect(input).toHaveValue('1234567890.12');
  });

  it('truncates long decimal numbers on paste', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    // Simulate paste and change
    fireEvent.paste(input, {
      clipboardData: {
        getData: () => '123.456789',
      },
    });

    fireEvent.change(input, {
      target: { value: '123.456789' },
    });

    // Trigger blur to format the value
    fireEvent.blur(input);

    expect(input).toHaveValue('123.46');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: 123.456789 }),
      }),
    );
  });

  it('filters out non-numeric characters while typing', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput onChange={onChange} />);
    const input = screen.getByRole('textbox');

    await user.type(input, '12a34b.c56');
    expect(input).toHaveValue('1234.56');
    expect(onChange).toHaveBeenLastCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: 1234.56 }),
      }),
    );
  });

  it('preserves valid numbers when typing invalid characters in middle', async () => {
    const user = userEvent.setup();
    render(<MoneyValueInput value={123.45} />);
    const input = screen.getByRole('textbox') as HTMLInputElement;

    // Place cursor in middle and type invalid chars
    input.setSelectionRange(2, 2);
    await user.type(input, 'abc');
    expect(input).toHaveValue('123.45');
  });

  it('handles paste at cursor position', async () => {
    const onChange = vi.fn();
    render(<MoneyValueInput value={123.45} onChange={onChange} />);
    const input = screen.getByRole('textbox') as HTMLInputElement;

    // Position cursor and paste
    input.setSelectionRange(2, 2);

    fireEvent.paste(input, {
      clipboardData: {
        getData: () => '99',
      },
    });

    fireEvent.change(input, {
      target: { value: '12993.45' },
    });

    expect(input).toHaveValue('12993.45');
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        target: expect.objectContaining({ value: 12993.45 }),
      }),
    );
  });

  it('respects disabled state', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<MoneyValueInput disabled value={123.45} onChange={onChange} />);
    const input = screen.getByRole('textbox');

    expect(input).toBeDisabled();
    await user.type(input, '999');
    expect(input).toHaveValue('123.45');
    expect(onChange).not.toHaveBeenCalled();
  });
});
