import { useErrorBoundary } from 'react-error-boundary';

export const useResetErrorBoundary = () => {
  const { resetBoundary } = useErrorBoundary();
  return resetBoundary;
};
