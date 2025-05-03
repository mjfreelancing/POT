import { ErrorDialog } from '../dialog/errorDialog';

// Can be (error: Error, errorInfo: ErrorInfo)
export const logError = (error: Error) => {
  // TODO: Do something with this error
  console.info(`Error: ${error.message}`);
  // console.info(`Stack: ${errorInfo.componentStack}`);
};

type FallbackErrorDialogProps = {
  error: Error;
  resetErrorBoundary: () => void;
};

export function ErrorDialogFallback({
  error,
  resetErrorBoundary,
}: FallbackErrorDialogProps) {
  return (
    <ErrorDialog
      open={true}
      title="An Error Occurred"
      description={error.message}
      onOk={resetErrorBoundary}
    />
  );
}
