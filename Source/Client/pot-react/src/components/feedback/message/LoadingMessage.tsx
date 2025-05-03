import { useEffect, useRef, useState } from 'react';

import Spinner from '../spinner/LoadingSpinner';

type LoadingMessageProps = {
  isLoading: boolean;
  text: string;
  delay?: number;
};

function LoadingMessage({ isLoading, text, delay = 300 }: LoadingMessageProps) {
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
