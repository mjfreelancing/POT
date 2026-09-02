import type { ComponentPropsWithoutRef } from 'react';
import {
  forwardRef,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';

import { Input } from '../ui/input';

const MAX_BSB_DIGITS = 6;

function getDigitsOnly(rawValue: string): string {
  return rawValue.replace(/\D/g, '').slice(0, MAX_BSB_DIGITS);
}

function formatBsb(digits: string): string {
  if (digits.length <= 3) {
    return digits;
  }

  return `${digits.slice(0, 3)}-${digits.slice(3)}`;
}

function normalizeBsbValue(value: string | undefined): string {
  return formatBsb(getDigitsOnly(value ?? ''));
}

function countDigits(text: string): number {
  let digitCount = 0;

  for (const character of text) {
    if (character >= '0' && character <= '9') {
      digitCount += 1;
    }
  }

  return digitCount;
}

function getCaretAfterDigitCount(
  formattedValue: string,
  digitCount: number,
): number {
  let seenDigits = 0;

  for (let index = 0; index < formattedValue.length; index += 1) {
    const character = formattedValue[index];

    if (character >= '0' && character <= '9') {
      seenDigits += 1;
    }

    if (seenDigits === digitCount) {
      return index + 1;
    }
  }

  return formattedValue.length;
}

// 'type', 'inputMode', and 'maxLength' are fixed internally; values are always displayed
// and emitted in the canonical XXX-XXX format regardless of how the user types them.
type BsbInputProps = Omit<
  ComponentPropsWithoutRef<typeof Input>,
  'type' | 'inputMode' | 'maxLength'
> & {
  value?: string | undefined;
};

const BsbInput = forwardRef<HTMLInputElement, BsbInputProps>(function BsbInput(
  { value, onChange, onBlur, onKeyDown, ...props },
  ref,
) {
  const [isUserInput, setIsUserInput] = useState(false);
  const [displayValue, setDisplayValue] = useState(() =>
    normalizeBsbValue(value),
  );
  const inputElementRef = useRef<HTMLInputElement | null>(null);
  const caretRef = useRef<number | null>(null);

  useEffect(() => {
    // Only sync the display from the external value while the user is not typing.
    // This prevents async value updates from clobbering an in-progress edit.
    if (isUserInput) {
      return;
    }

    const nextDisplayValue = normalizeBsbValue(value);

    setDisplayValue(nextDisplayValue);
  }, [isUserInput, value]);

  useLayoutEffect(() => {
    const nextCaret = caretRef.current;
    const inputElement = inputElementRef.current;

    if (nextCaret === null || inputElement === null) {
      return;
    }

    // Restore the caret to the position that matches the number of digits typed
    // so the auto-inserted dash does not cause the caret to drift.
    caretRef.current = null;
    inputElement.setSelectionRange(nextCaret, nextCaret);
  }, [displayValue]);

  const handleRef = (inputElement: HTMLInputElement | null) => {
    inputElementRef.current = inputElement;

    if (typeof ref === 'function') {
      ref(inputElement);
    } else if (ref) {
      ref.current = inputElement;
    }
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setIsUserInput(true);

    const rawValue = event.target.value;
    const caretStart = event.target.selectionStart ?? rawValue.length;
    const digitCountBeforeCaret = countDigits(rawValue.slice(0, caretStart));
    const nextDisplayValue = normalizeBsbValue(rawValue);

    setDisplayValue(nextDisplayValue);

    caretRef.current = getCaretAfterDigitCount(
      nextDisplayValue,
      digitCountBeforeCaret,
    );

    // Rewrite the emitted value to the canonical format so the caller (for example
    // react-hook-form) always stores XXX-XXX, matching the server contract.
    onChange?.({
      ...event,
      target: {
        ...event.target,
        value: nextDisplayValue,
      },
    });
  };

  const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
    setIsUserInput(false);
    onBlur?.(event);
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    const { key } = event;
    const isSingleCharacter = key.length === 1;
    const isDigit = key >= '0' && key <= '9';
    const isShortcut = event.ctrlKey || event.metaKey;

    // Allow digits, the dash, navigation/editing keys, and ctrl/meta shortcuts.
    // Blocking stray printable characters avoids distracting caret jumps.
    if (isSingleCharacter && !isDigit && key !== '-' && !isShortcut) {
      event.preventDefault();
      return;
    }

    onKeyDown?.(event);
  };

  return (
    <Input
      {...props}
      ref={handleRef}
      value={displayValue}
      type="text"
      inputMode="numeric"
      maxLength={7}
      onChange={handleChange}
      onBlur={handleBlur}
      onKeyDown={handleKeyDown}
    />
  );
});

export default BsbInput;
export type { BsbInputProps };
