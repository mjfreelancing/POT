import { ComponentPropsWithoutRef, useEffect, useState } from 'react';

import { isNumber } from '@/lib/utils';

import { Input } from '../ui/input';

// The onChange event will pass both the original string value and a parsed number value
export type MoneyValueInputElement = HTMLInputElement & {
  number: number | undefined;
};

export type MoneyValueChangeEvent = Omit<
  React.ChangeEvent<HTMLInputElement>,
  'target'
> & {
  target: MoneyValueInputElement;
};

// 'type' and 'inputMode' are defined internally
// 'min' and 'max' are not supported - caller can use custom validation
// 'onChange' is overridden to pass both string and number values
type MoneyValueInputProps = Omit<
  ComponentPropsWithoutRef<typeof Input>,
  'type' | 'inputMode' | 'min' | 'max' | 'onChange' | 'value'
> & {
  value?: string | number | undefined;
  onChange?: (event: MoneyValueChangeEvent) => void;
};

const isIncompleteValue = (value: string) =>
  value === '' || value === '.' || value === '-';

const formatDisplayValue = (value: string | number | undefined): string => {
  const numericalValue = parseNumericValue(value);
  return numericalValue !== undefined ? numericalValue.toFixed(2) : '';
};

// Helper function to safely parse numeric strings and numbers to a numeric value.
// Anything that can't be parsed to a number (including dot and minus) will return undefined.
const parseNumericValue = (
  value: string | number | undefined,
): number | undefined => {
  if (value === undefined) {
    return undefined;
  }

  if (typeof value === 'string' && isIncompleteValue(value)) {
    return undefined;
  }

  if (isNumber(value)) {
    return value;
  }

  const parsedValue = parseFloat(value as string);
  return isNaN(parsedValue) ? undefined : parsedValue;
};

function MoneyValueInput({
  value,
  onChange,
  onBlur,
  onKeyDown,
  ...props
}: MoneyValueInputProps) {
  const [isUserInput, setIsUserInput] = useState(false);

  const [displayValue, setDisplayValue] = useState(() => {
    // Initialize with empty string rather than undefined to avoid uncontrolled->controlled warnings in tests
    return formatDisplayValue(value);
  });

  useEffect(() => {
    // Only update the display value from the external value prop when it's not being edited by the user.
    if (isUserInput) {
      return;
    }

    const formattedValue = formatDisplayValue(value);

    setDisplayValue(formattedValue);
  }, [isUserInput, value]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    // Flag the user is typing
    setIsUserInput(true);

    const newValue = e.target.value;

    // Always update displayValue to ensure controlled input shows what the user typed
    setDisplayValue(newValue);

    onChange?.({
      ...e,
      target: {
        ...e.target,
        number: parseNumericValue(newValue),
      },
    } as MoneyValueChangeEvent);
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    // Flag the user is no longer typing. Will result in the display value being updated
    setIsUserInput(false);

    onChange?.({
      ...e,
      target: {
        ...e.target,
        number: parseNumericValue(displayValue),
      },
    } as MoneyValueChangeEvent);

    // Call the original onBlur handler
    onBlur?.(e);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    const allowedKeys = [
      'Backspace',
      'Delete',
      'Tab',
      'Escape',
      'Enter',
      '.',
      '-',
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      'ArrowLeft',
      'ArrowRight',
    ];

    // Block non-allowed keys first
    if (!allowedKeys.includes(e.key) && !e.ctrlKey && !e.metaKey) {
      e.preventDefault();
      return;
    }

    const currentValue = e.currentTarget.value;
    const selectionStart = e.currentTarget.selectionStart || 0;
    const selectionEnd = e.currentTarget.selectionEnd || 0;
    const hasSelection = selectionEnd > selectionStart;

    if (e.key === '-') {
      if (currentValue.includes('-') && !hasSelection) {
        // Prevent adding another minus if one already exists and not replacing selection
        e.preventDefault();
        return;
      }

      // Allow minus at position 0 or when entire field is selected
      const isAllTextSelected =
        selectionStart === 0 && selectionEnd === currentValue.length;

      if (selectionStart > 0 && !isAllTextSelected) {
        e.preventDefault();
        return;
      }
    }

    if (e.key === '.') {
      if (currentValue.includes('.') && !hasSelection) {
        // Prevent adding another decimal point if one already exists and not replacing selection
        e.preventDefault();
        return;
      }
    }

    onKeyDown?.(e);
  };

  return (
    // Tried using type="number" but it is inconsistent across browsers and it's impossible to detect "--"
    // being entered because when the input is "-", the value of e.currentTarget.value is "". Forced to
    // use type="text" and handle the formatting / validation internally.
    <Input
      {...props}
      value={displayValue}
      type="text"
      inputMode="decimal"
      onChange={handleChange}
      onBlur={handleBlur}
      onKeyDown={handleKeyDown}
    />
  );
}

export default MoneyValueInput;
