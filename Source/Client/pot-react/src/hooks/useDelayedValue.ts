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
  // Tracks whether the delay has elapsed during the CURRENT true episode. The value is
  // derived below (not stored), so flipping `condition` back to false reverts to
  // `initialValue` immediately with no state write in the effect.
  const [revealed, setRevealed] = useState(false);

  // Use a ref to store the delay value to avoid unnecessary re-renders - the value is never
  // changed so this avoids having to add it to the dependency array.
  const delayRef = useRef(delay);

  useEffect(() => {
    if (!condition) {
      return undefined;
    }

    const timeout = window.setTimeout(
      () => setRevealed(true),
      delayRef.current,
    );

    return () => {
      window.clearTimeout(timeout);
      // End of a true episode: drop the revealed flag so the next true episode starts
      // from `initialValue` and re-observes the full delay.
      setRevealed(false);
    };
  }, [condition]);

  return condition && revealed ? delayedValue : initialValue;
}

export default useDelayedValue;
export type { UseDelayedValueProps };
