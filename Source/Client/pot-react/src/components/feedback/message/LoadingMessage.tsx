import Spinner from '../spinner/LoadingSpinner';

type LoadingMessageProps = {
  isLoading?: boolean;
};

function LoadingMessage({ isLoading = true }: LoadingMessageProps) {
  if (!isLoading) {
    return null;
  }

  return (
    <div className="flex flex-col items-center justify-center mt-8">
      <div className="flex items-center">
        <Spinner />
        Loading...
      </div>
    </div>
  );
}

export default LoadingMessage;
export type { LoadingMessageProps };
