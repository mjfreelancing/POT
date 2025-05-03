import { useEffect, useRef, useState } from 'react';

import Spinner from '../spinner/LoadingSpinner';

/*
 * LoadingMessage serves two purposes:
 *
 * 1. Route-level loading (Suspense fallback):
 *    - Used when lazy-loaded route components are being downloaded
 *    - Mounted/unmounted automatically by Suspense
 *    - Uses default props (isLoading=true)
 *
 * 2. Data loading:
 *    - Used during API calls or other async operations
 *    - Controlled by the parent via isLoading prop
 *    - Displays after a brief delay to prevent flash for quick operations
 */

const DEFAULT_LOADING_DELAY = 300;
const DEFAULT_LOADING_TEXT = 'Loading...';

type LoadingMessageProps = {
  isLoading?: boolean;
  delay?: number;
  text?: string;
};

function LoadingMessage({
  isLoading = true,
  delay = DEFAULT_LOADING_DELAY,
  text = DEFAULT_LOADING_TEXT,
}: LoadingMessageProps) {
  const [showLoading, setShowLoading] = useState(false);

  // Use a ref to store the delay value to avoid unnecessary re-renders - the value is never
  // changed so this avoids having to add it to the dependency array.
  const delayRef = useRef(delay);

  useEffect(() => {
    let timeout: number;

    if (isLoading) {
      timeout = window.setTimeout(() => setShowLoading(true), delayRef.current);
    } else {
      setShowLoading(false);
    }

    return () => window.clearTimeout(timeout);
  }, [isLoading]);

  if (!showLoading) {
    return null;
  }

  return (
    <div className="flex flex-col items-center justify-center mt-8">
      <div className="flex items-center">
        <Spinner />
        {text}
      </div>
    </div>
  );
}

export default LoadingMessage;
