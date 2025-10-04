import { useCallback, useEffect, useState } from 'react';

import { logger } from '@/lib/logging';

/**
 * Hook to monitor window visibility state and notify when the window becomes visible
 * after being hidden. Note that in most cases, useWindowFocus is preferred since
 * window focus implies visibility.
 *
 * @param onVisible - Optional callback to execute when the window becomes visible
 * @returns isVisible - Current visibility state of the window
 */
function useWindowVisibility(onVisible?: () => void): boolean {
  const [isVisible, setIsVisible] = useState<boolean>(() => !document.hidden);

  const handleVisibilityChange = useCallback(() => {
    const nextVisible = !document.hidden;
    setIsVisible(nextVisible);

    if (nextVisible && onVisible) {
      logger.info(
        'useWindowVisibility',
        'Window became visible, triggering refresh',
      );
      onVisible();
    }
  }, [onVisible]);

  useEffect(() => {
    logger.info('useWindowVisibility', 'Setting up visibility change listener');
    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      logger.info(
        'useWindowVisibility',
        'Cleaning up visibility change listener',
      );
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [handleVisibilityChange]);

  return isVisible;
}

export { useWindowVisibility };
