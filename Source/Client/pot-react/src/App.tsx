import { type ReactNode, useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';

import { ErrorSheet } from '@/components/feedback';
import { Toaster } from '@/components/ui/sonner';
import { logger } from '@/concerns';
import { buildEnvScopedKey, buildUserScopedKey } from '@/concerns/storage';
import type { DisplayError } from '@/lib';

import { AppSidebar } from './components/nav';
import { ThemeProvider } from './components/theme';
import { SidebarProvider } from './components/ui/sidebar';
import { ErrorProvider } from './contexts';
import { AuthProvider } from './features/auth/contexts';
import { AppRoutes } from './routes/AppRoutes';
import { useUserStore } from './stores';

const AppContent = () => (
  <SidebarProvider>
    {/* Use the full viewport height. Will get 2 columns with the sidebar on the left and other content on the right */}
    <div className="flex flex-1 bg-gradient-to-br from-background via-background to-muted/20 min-w-0">
      <AppSidebar />
      {/* The routed pages will expand and fill the remaining space */}
      <div className="flex-1 relative min-w-0">
        {/* Subtle background pattern */}
        <div className="absolute inset-0 opacity-[0.02] bg-[radial-gradient(circle_at_1px_1px,_oklch(var(--foreground))_1px,_transparent_0)] [background-size:20px_20px]" />
        {/*
          Toaster must live outside the z-10 div below. That div creates a CSS stacking context
          (position: relative + z-index), which would confine Sonner's z-index: 999999999 to a
          z-10 paint layer. Radix portals (Sheet, Dialog) render at document.body level with z-50,
          which would paint above the toasts. Placing Toaster here avoids that stacking context —
          this div has no z-index, so Sonner's fixed position is evaluated at the root level.
        */}
        <Toaster position="top-center" />
        <div className="relative z-10 h-full">
          <AppRoutes />
        </div>
      </div>
    </div>
  </SidebarProvider>
);

function ThemedApp({ children }: { children: ReactNode }) {
  const userId = useUserStore(store => store.userInfo?.rowId);
  const storageKey = userId
    ? buildUserScopedKey({ userId, feature: 'theme' })
    : buildEnvScopedKey('theme');

  return (
    <ThemeProvider
      key={userId ?? 'global-theme'}
      defaultTheme="system"
      storageKey={storageKey}
    >
      {children}
    </ThemeProvider>
  );
}

const App = () => {
  logger.info('App', `Running mode: ${import.meta.env.MODE}`);
  const [error, setError] = useState<DisplayError | null>();

  const handleError = (error: Error) => {
    logger.error('App', 'Error boundary caught an error', error);

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
    <ErrorProvider>
      <AuthProvider>
        <ThemedApp>
          <div className="flex h-screen w-screen overflow-hidden">
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
              onDismiss={() => setError(null)}
            />
          )}
        </ThemedApp>
      </AuthProvider>
    </ErrorProvider>
  );
};

export default App;
