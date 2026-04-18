import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { describe, expect, test, vi } from 'vitest';

import SearchInput from '@/components/filters/SearchInput';

describe('SearchInput', () => {
  test('renders current value and calls onChange as user types', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    function ControlledSearchInput() {
      const [value, setValue] = React.useState('');

      return (
        <SearchInput
          value={value}
          onChange={nextValue => {
            onChange(nextValue);
            setValue(nextValue);
          }}
          placeholder="Search expenses"
          ariaLabel="Search expenses"
        />
      );
    }

    render(<ControlledSearchInput />);

    const input = screen.getByRole('textbox', { name: 'Search expenses' });

    await user.type(input, 'rent');

    expect(onChange).toHaveBeenNthCalledWith(1, 'r');
    expect(onChange).toHaveBeenNthCalledWith(2, 're');
    expect(onChange).toHaveBeenNthCalledWith(3, 'ren');
    expect(onChange).toHaveBeenNthCalledWith(4, 'rent');
  });

  test('shows clear button when value exists and clears on click', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(
      <SearchInput
        value="rent"
        onChange={onChange}
        placeholder="Search expenses"
        ariaLabel="Search expenses"
      />,
    );

    const clearButton = screen.getByRole('button', {
      name: 'Clear search input',
    });

    expect(clearButton).toBeInTheDocument();

    await user.click(clearButton);

    expect(onChange).toHaveBeenCalledWith('');
  });

  test('hides clear button when disabled', () => {
    render(
      <SearchInput
        value="rent"
        onChange={vi.fn()}
        placeholder="Search expenses"
        ariaLabel="Search expenses"
        disabled={true}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Clear search input' }),
    ).not.toBeInTheDocument();
  });

  test('applies active ring style when input has non-whitespace value', () => {
    render(
      <SearchInput
        value="rent"
        onChange={vi.fn()}
        placeholder="Search expenses"
        ariaLabel="Search expenses"
      />,
    );

    expect(
      screen.getByRole('textbox', { name: 'Search expenses' }),
    ).toHaveClass('ring-[2px]', 'ring-primary/60', 'bg-primary/10');
  });
});
