import { useCallback, useEffect, useState } from 'react';
import { logger } from '@/lib/logging';

/**
 * Hook that monitors window focus state.
 * Triggers the callback when the window regains focus, which implicitly means
 * the page is also visible (a window cannot have focus while being hidden).
 *
 * @param onFocus - Optional callback to execute when the window gains focus
 * @param options - Configuration options
 * @returns isFocused - Current focus state of the window
 */
function useWindowFocus(
  onFocus?: () => void,
  options: { refetchOnWindowFocus?: boolean } = { refetchOnWindowFocus: true },
): boolean {
  const [isFocused, setIsFocused] = useState<boolean>(() =>
    document.hasFocus(),
  );

  const handleFocusChange = useCallback(
    (focused: boolean) => {
      setIsFocused(focused);

      if (focused && options.refetchOnWindowFocus && onFocus) {
        logger.info('useWindowFocus', 'Window focused, triggering refresh');
        onFocus();
      }
    },
    [onFocus, options.refetchOnWindowFocus],
  );

  useEffect(() => {
    logger.info('useWindowFocus', 'Setting up focus listeners');

    const handleFocus = () => handleFocusChange(true);
    const handleBlur = () => handleFocusChange(false);

    window.addEventListener('focus', handleFocus);
    window.addEventListener('blur', handleBlur);

    return () => {
      logger.info('useWindowFocus', 'Cleaning up focus listeners');
      window.removeEventListener('focus', handleFocus);
      window.removeEventListener('blur', handleBlur);
    };
  }, [handleFocusChange]);

  return isFocused;
}

export { useWindowFocus };
