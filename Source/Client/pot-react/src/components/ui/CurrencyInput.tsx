import { ComponentPropsWithoutRef, useState } from 'react';

import { isNumber } from '@/lib/utils';

import { Input } from './shadcn/input';

type CurrencyInputProps = Omit<
  ComponentPropsWithoutRef<typeof Input>,
  'type' | 'inputMode'
>;

function CurrencyInput({
  value,
  min,
  onChange,
  onBlur,
  onKeyDown,
  ...props
}: CurrencyInputProps) {
  const [displayValue, setDisplayValue] = useState(() =>
    isNumber(value) ? value.toFixed(2) : undefined,
  );

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setDisplayValue(newValue);

    const numericValue = newValue === '' ? undefined : Number(newValue);

    onChange?.({
      ...e,
      target: { ...e.target, value: numericValue?.toString() || '' },
    } as React.ChangeEvent<HTMLInputElement>);
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    if (!displayValue) {
      setDisplayValue('');
    } else {
      const numValue = parseFloat(displayValue);
      setDisplayValue(numValue.toFixed(2));
    }
    onBlur?.(e);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (min !== undefined && e.key === '-') {
      const minValue = typeof min === 'number' ? min : parseFloat(min);

      if (minValue >= 0) {
        e.preventDefault();
      }
    }

    if (e.key === '.') {
      const [, decimals] = e.currentTarget.value.split('.');

      if (decimals?.length >= 2) {
        e.preventDefault();
      }
    }

    onKeyDown?.(e);
  };

  return (
    <Input
      {...props}
      value={displayValue}
      type="number"
      {...(min !== undefined && { min })} // only provide min if it's defined
      inputMode="decimal"
      onChange={handleChange}
      onBlur={handleBlur}
      onKeyDown={handleKeyDown}
    />
  );
}

export default CurrencyInput;
