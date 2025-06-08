import { useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';

import { ErrorSheet } from '@/components/feedback';
import { DisplayError } from '@/lib/errors/displayError';

import { AppSidebar } from './components/nav';
import { ThemeProvider } from './components/theme';
import { SidebarProvider } from './components/ui/sidebar';
import { AppRoutes } from './routes/AppRoutes';

const AppContent = () => (
  <SidebarProvider>
    {/* Use the full viewport height. Will get 2 columns with the sidebar on the left and other content on the right */}
    <div className="flex flex-1">
      <AppSidebar />
      {/* The routed pages will expand and fill the remaining space */}
      <div className="flex-1">
        <AppRoutes />
      </div>
    </div>
  </SidebarProvider>
);

const App = () => {
  console.info(`Running mode: ${import.meta.env.MODE}`);

  const [error, setError] = useState<DisplayError | undefined>();

  const handleError = (error: Error) => {
    console.error('Error boundary caught an error:', error);

    setError({
      title: 'Application Error',
      description: error.message,
    });
  };

  const getErrorWidthClass = (message: string): string => {
    if (message.length > 200) return 'max-w-4xl';
    if (message.length > 100) return 'max-w-2xl';
    if (message.length > 50) return 'max-w-xl';
    return 'max-w-md';
  };

  return (
    <ThemeProvider defaultTheme="system" storageKey="pot-ui-theme">
      <div className="flex h-screen w-screen">
        <ErrorBoundary
          fallbackRender={({ error }) => (
            <div
              role="alert"
              className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 p-4"
            >
              <div
                className={`bg-white dark:bg-gray-800 text-red-700 p-6 rounded-lg shadow-lg ${getErrorWidthClass(error.message)} w-full text-center`}
              >
                <p className="text-lg font-medium mb-2">
                  Something went wrong !
                </p>
                <pre className="whitespace-pre-wrap">{error.message}</pre>
              </div>
            </div>
          )}
          onError={handleError}
        >
          <AppContent />
        </ErrorBoundary>
      </div>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(undefined)}
        />
      )}
    </ThemeProvider>
  );
};

export default App;
