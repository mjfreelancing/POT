import { ErrorInfo } from 'react';
import { ErrorDialog } from '../dialogs/errorDialog';

export const logError = (error: Error, _: ErrorInfo) => {
  // TODO: Do something with this error
  console.info(`Error: ${error.message}`);
  // console.info(`Stack: ${errorInfo.componentStack}`);
};

type FallbackErrorDialogProps = {
  error: Error;
  resetErrorBoundary: () => void;
};

export function FallbackErrorDialog({
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
