import { ErrorBoundary } from 'react-error-boundary';

import {
  ErrorDialogFallback,
  logError,
} from './components/errorBoundary/ErrorDialogFallback';
import { AppLayout } from './components/layout/AppLayout';
import { AppSidebar } from './components/nav/AppSidebar';
import { ThemeProvider } from './components/theme/ThemeProvider';
import { SidebarProvider } from './components/ui/sidebar';
import { AppRoutes } from './routes/AppRoutes';

const App = () => {
  console.info(`Running mode: ${import.meta.env.MODE}`);

  return (
    <ThemeProvider defaultTheme="system" storageKey="pot-ui-theme">
      <div className="flex h-screen w-screen">
        <ErrorBoundary
          FallbackComponent={({ error, resetErrorBoundary }) => (
            <ErrorDialogFallback
              error={error}
              resetErrorBoundary={resetErrorBoundary}
            >
              <SidebarProvider>
                <AppSidebar />
                <AppLayout>
                  <AppRoutes />
                </AppLayout>
              </SidebarProvider>
            </ErrorDialogFallback>
          )}
          onError={logError}
        >
          <SidebarProvider>
            <AppSidebar />
            <AppLayout>
              <AppRoutes />
            </AppLayout>
          </SidebarProvider>
        </ErrorBoundary>
      </div>
    </ThemeProvider>
  );
};

export default App;
