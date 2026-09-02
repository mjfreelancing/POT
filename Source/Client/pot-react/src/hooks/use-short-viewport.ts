import { useEffect, useState } from 'react';

/**
 * Maximum viewport height (px) at which the fixed-height list page layout
 * (page header + toolbar + paddings + bulk-actions bar) still leaves enough
 * room for an internally scrolling table/card region. Below this height the
 * flex region collapses (often to 0px) on short viewports such as a phone
 * rotated to landscape, so list pages fall back to scrolling their whole
 * content area to keep rows reachable.
 */
const SHORT_VIEWPORT_MAX_HEIGHT = 560;

/**
 * Returns true while the viewport is too short for the fixed header + toolbar +
 * internal-table-scroll list layout to leave a usable table region. Reactive to
 * window resizes and orientation changes.
 */
export function useIsShortViewport() {
  const [isShortViewport, setIsShortViewport] = useState<
    boolean | undefined
  >(undefined);

  useEffect(() => {
    const mql = window.matchMedia(
      `(max-height: ${SHORT_VIEWPORT_MAX_HEIGHT - 1}px)`,
    );

    const onChange = () => {
      setIsShortViewport(window.innerHeight < SHORT_VIEWPORT_MAX_HEIGHT);
    };

    mql.addEventListener('change', onChange);
    setIsShortViewport(window.innerHeight < SHORT_VIEWPORT_MAX_HEIGHT);

    return () => mql.removeEventListener('change', onChange);
  }, []);

  return !!isShortViewport;
}
