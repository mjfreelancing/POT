import { useDelayedBooleanState } from '../../../hooks/useDelayedState';
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

export type LoadingMessageProps = {
  isLoading?: boolean;
  delay?: number;
  text?: string;
};

function LoadingMessage({
  isLoading = true,
  delay = DEFAULT_LOADING_DELAY,
  text = DEFAULT_LOADING_TEXT,
}: LoadingMessageProps) {
  const showLoading = useDelayedBooleanState({
    condition: isLoading,
    delay,
  });

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
