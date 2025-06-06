import { useEffect, useRef, useState } from 'react';

type UseDelayedStateProps<T> = {
  condition: boolean;
  delay: number;
  initialValue: T;
  delayedValue: T;
};

/**
 * Custom hook that manages a delayed state change based on a condition.
 *
 * When condition becomes true: initialValue → (after delay) → delayedValue
 * When condition becomes false: immediately → initialValue
 *
 * @param condition - The condition that triggers the delayed state change
 * @param delay - The delay in milliseconds before the state changes
 * @param initialValue - The initial/reset value for the state
 * @param delayedValue - The value to change to after the delay
 * @returns The current state value
 */
function useDelayedState<T>({
  condition,
  delay,
  initialValue,
  delayedValue,
}: UseDelayedStateProps<T>): T {
  const [state, setState] = useState<T>(initialValue);

  // Use a ref to store the delay value to avoid unnecessary re-renders - the value is never
  // changed so this avoids having to add it to the dependency array.
  const delayRef = useRef(delay);

  useEffect(() => {
    let timeout: number;

    if (condition) {
      timeout = window.setTimeout(
        () => setState(delayedValue),
        delayRef.current,
      );
    } else {
      setState(initialValue);
    }

    return () => window.clearTimeout(timeout);
  }, [condition, initialValue, delayedValue]);

  return state;
}

// Convenience function for the common boolean case
type UseDelayedBooleanStateProps = {
  condition: boolean;
  delay: number;
};

/**
 * Convenience wrapper for boolean delayed state (false → true after delay).
 *
 * @param condition - The condition that triggers the delayed state change
 * @param delay - The delay in milliseconds before showing
 * @returns Boolean state (false initially, true after delay when condition is true)
 */
function useDelayedBooleanState({
  condition,
  delay,
}: UseDelayedBooleanStateProps): boolean {
  return useDelayedState({
    condition,
    delay,
    initialValue: false,
    delayedValue: true,
  });
}

export { useDelayedBooleanState };
export default useDelayedState;
