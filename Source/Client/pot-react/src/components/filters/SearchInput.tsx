import { X } from 'lucide-react';
import React from 'react';

import { Input } from '@/components/ui/input';

// Generic search input with a clear (x) button
export type SearchInputProps = {
  value: string;
  onChange: (value: string) => void;
  placeholder: string;
  ariaLabel: string;
  name?: string;
  className?: string;
  disabled?: boolean;
};

/**
 * Renders a search input with a clear (x) button.
 * The clear button appears only when the input is non-empty.
 */
function SearchInput({
  value,
  onChange,
  placeholder,
  ariaLabel,
  name,
  className,
  disabled = false,
}: SearchInputProps) {
  function handleInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    onChange(e.target.value);
  }

  function handleClear() {
    onChange('');
  }

  return (
    <div className="relative w-80">
      <Input
        type="text"
        value={value}
        onChange={handleInputChange}
        placeholder={placeholder}
        aria-label={ariaLabel}
        name={name}
        className={className}
        disabled={disabled}
        autoComplete="off"
      />
      {value && !disabled && (
        <button
          type="button"
          aria-label="Clear search input"
          onClick={handleClear}
          tabIndex={0}
          className="absolute right-2 top-1/2 -translate-y-1/2 p-1 rounded hover:bg-muted focus:bg-muted focus:outline-none"
        >
          <X className="w-4 h-4 text-muted-foreground" />
        </button>
      )}
    </div>
  );
}

export default SearchInput;
