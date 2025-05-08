import { useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';

import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { DisplayError } from '@/lib/errors/displayError';

import { AppLayout } from './components/layout/AppLayout';
import { AppSidebar } from './components/nav/AppSidebar';
import { ThemeProvider } from './components/theme/ThemeProvider';
import { SidebarProvider } from './components/ui/sidebar';
import { AppRoutes } from './routes/AppRoutes';

const AppContent = () => (
  <SidebarProvider>
    <AppSidebar />
    <AppLayout>
      <AppRoutes />
    </AppLayout>
  </SidebarProvider>
);

const App = () => {
  console.info(`Running mode: ${import.meta.env.MODE}`);

  const [error, setError] = useState<DisplayError | null>(null);

  const handleError = (error: Error) => {
    console.error('Error boundary caught an error:', error);

    setError({
      title: 'Application Error',
      description: error.message,
    });
  };

  return (
    <ThemeProvider defaultTheme="system" storageKey="pot-ui-theme">
      <div className="flex h-screen w-screen">
        <ErrorBoundary fallback={<AppContent />} onError={handleError}>
          <AppContent />
        </ErrorBoundary>
      </div>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </ThemeProvider>
  );
};

export default App;
