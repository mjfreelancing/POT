import { useErrorBoundary } from 'react-error-boundary';

function useResetErrorBoundary() {
  const { resetBoundary } = useErrorBoundary();
  return resetBoundary;
}

export default useResetErrorBoundary;
