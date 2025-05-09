import { ComponentPropsWithoutRef, useState, useEffect } from 'react';

import { isNumber } from '@/lib/utils';

import { Input } from '../ui/input';

// Th onChange event will pass a number value rather than a string (or undefined)
type MoneyValueInputElement = Omit<HTMLInputElement, 'value'> & {
  value: number | undefined;
};

type MoneyValueChangeEvent = Omit<
  React.ChangeEvent<HTMLInputElement>,
  'target'
> & {
  target: MoneyValueInputElement;
};

// 'type' and 'inputMode' are defined internally
// 'min' and 'max' are not supported - caller can use custom validation
// 'onChange' is overridden to pass a number value rather than a string (or undefined)
type MoneyValueInputProps = Omit<
  ComponentPropsWithoutRef<typeof Input>,
  'type' | 'inputMode' | 'min' | 'max' | 'onChange'
> & {
  onChange?: (event: MoneyValueChangeEvent) => void;
};

function MoneyValueInput({
  value,
  onChange,
  onBlur,
  onKeyDown,
  ...props
}: MoneyValueInputProps) {
  const [displayValue, setDisplayValue] = useState(() =>
    isNumber(value) ? value.toFixed(2) : undefined,
  );

  useEffect(() => {
    if (isNumber(value)) {
      console.info('MoneyValueInput value update:', { value });
      setDisplayValue(value.toFixed(2));
    }
  }, [value]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    const isIncompleteValue =
      newValue === '' || newValue === '-' || newValue === '.';

    // Only allow valid decimal number formats
    if (isIncompleteValue || /^-?\d*\.?\d*$/.test(newValue)) {
      setDisplayValue(newValue);

      const numericValue = isIncompleteValue ? undefined : Number(newValue);

      onChange?.({
        ...e,
        target: { ...e.target, value: numericValue },
      } as MoneyValueChangeEvent);
    }
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    if (!displayValue || displayValue === '-') {
      setDisplayValue('');
    } else {
      const numValue = parseFloat(displayValue);
      if (!isNaN(numValue)) {
        setDisplayValue(numValue.toFixed(2));
      } else {
        setDisplayValue('');
      }
    }
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
    // being entered because when the input is "-", the value of e.currentTarget.value is "". Force to
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
