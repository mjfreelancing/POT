import type { ReactNode } from 'react';
import { createContext, useContext, useState } from 'react';

import type { DisplayError } from '@/lib';

type ErrorContextType = {
  error: DisplayError | null;
  setError: (error: DisplayError | null) => void;
};

const ErrorContext = createContext<ErrorContextType | undefined>(undefined);

const ErrorProvider = ({ children }: { children: ReactNode }) => {
  const [error, setError] = useState<DisplayError | null>(null);

  return (
    <ErrorContext.Provider value={{ error, setError }}>
      {children}
    </ErrorContext.Provider>
  );
};

const useErrorContext = (): ErrorContextType => {
  const context = useContext(ErrorContext);
  if (!context) {
    throw new Error('useErrorContext must be used within an ErrorProvider');
  }
  return context;
};

export { ErrorProvider, useErrorContext };
