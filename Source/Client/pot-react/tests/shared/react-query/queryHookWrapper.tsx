import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';

type QueryHookWrapperOptions = {
  queryClient?: QueryClient;
  retry?: boolean;
};

function createQueryClient(options: { retry?: boolean } = {}): QueryClient {
  const { retry = false } = options;

  return new QueryClient({
    defaultOptions: {
      queries: {
        retry,
      },
    },
  });
}

function createQueryHookWrapper(options: QueryHookWrapperOptions = {}) {
  const queryClient =
    options.queryClient ?? createQueryClient({ retry: options.retry });

  return ({ children }: { children: ReactNode }) => {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}

export { createQueryClient, createQueryHookWrapper };
