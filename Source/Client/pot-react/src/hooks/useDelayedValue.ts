import { useEffect, useRef, useState } from 'react';

type UseDelayedValueProps<T> = {
  condition: boolean;
  delay: number;
  initialValue: T;
  delayedValue: T;
};

/**
 * Custom hook that manages a delayed value change based on a condition.
 *
 * When condition becomes true: initialValue → (after delay) → delayedValue
 * When condition becomes false: immediately → initialValue
 *
 * @param condition - The condition that triggers the delayed value change
 * @param delay - The delay in milliseconds before the value changes
 * @param initialValue - The initial value
 * @param delayedValue - The value to change to after the delay
 * @returns The current value
 */
function useDelayedValue<T>({
  condition,
  delay,
  initialValue,
  delayedValue,
}: UseDelayedValueProps<T>): T {
  const [value, setValue] = useState<T>(initialValue);

  // Use a ref to store the delay value to avoid unnecessary re-renders - the value is never
  // changed so this avoids having to add it to the dependency array.
  const delayRef = useRef(delay);

  useEffect(() => {
    let timeout: number;

    if (condition) {
      timeout = window.setTimeout(
        () => setValue(delayedValue),
        delayRef.current,
      );
    } else {
      setValue(initialValue);
    }

    return () => window.clearTimeout(timeout);
  }, [condition, initialValue, delayedValue]);

  return value;
}

export default useDelayedValue;
export type { UseDelayedValueProps };
