import { useEffect, useState } from 'react';

type LoadingMessageProps = {
  isLoading: boolean;
  text: string;
};

function LoadingMessage({ isLoading, text }: LoadingMessageProps) {
  const [showLoading, setShowLoading] = useState(false);

  useEffect(() => {
    let timeout: number;

    if (isLoading) {
      timeout = window.setTimeout(() => setShowLoading(true), 300);
    } else {
      setShowLoading(false);
    }

    return () => window.clearTimeout(timeout);
  }, [isLoading]);

  if (!showLoading) {
    return null;
  }

  return (
    <div className="flex flex-col items-center justify-center mt-8">{text}</div>
  );
}

export default LoadingMessage;
