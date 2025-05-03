import { createPortal } from 'react-dom';
import { ErrorDialog } from '../dialog/errorDialog';

// Can be (error: Error, errorInfo: ErrorInfo)
// console.info(`Stack: ${errorInfo.componentStack}`);
export const logError = (error: Error) => {
  // TODO: Do something with this error, such as send to Sentry
  console.info(`Error: ${error.message}`);
};

// The ErrorDialogFallback component:
// - Preserves the existing UI when an error occurs
// - Shows an error dialog overlay on top of the preserved UI

type ErrorDialogFallbackProps = {
  error: Error;
  resetErrorBoundary: () => void;
  children?: React.ReactNode;
};

export function ErrorDialogFallback({
  error,
  resetErrorBoundary,
  children,
}: ErrorDialogFallbackProps) {
  return (
    <>
      {/* Render the existing UI first, preserving the current application state */}
      {children}

      {/* 
        createPortal allows us to render content outside the normal React tree hierarchy.
        Instead of rendering the error dialog as a child of this component (which could be
        affected by parent styling/layout), we render it directly into document.body.
        
        This ensures the error dialog:
        1. Appears on top of everything else
        2. Is not constrained by parent container styles
        3. Maintains proper z-index stacking
        
        The dialog appears after {children} in the code, but visually appears on top
        due to being portaled to document.body
      */}
      {createPortal(
        <ErrorDialog
          open={true}
          title="An Error Occurred"
          description={error.message}
          onOk={resetErrorBoundary}
        />,
        document.body,
      )}
    </>
  );
}
