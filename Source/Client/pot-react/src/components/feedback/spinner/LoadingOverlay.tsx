import LoadingSpinner from './LoadingSpinner';

type LoadingOverlayProps = {
  message?: string;
};

function LoadingOverlay({ message = 'Loading...' }: LoadingOverlayProps) {
  return (
    <div className="absolute inset-0 bg-background/80 backdrop-blur-sm flex items-start justify-center pt-16 z-50">
      <div className="flex items-center gap-2 bg-card p-4 rounded-lg shadow-lg border">
        <LoadingSpinner />
        <span className="text-sm font-medium">{message}</span>
      </div>
    </div>
  );
}

export type { LoadingOverlayProps };
export default LoadingOverlay;
