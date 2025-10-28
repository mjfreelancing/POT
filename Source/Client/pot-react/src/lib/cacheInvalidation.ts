import type { QueryClient } from '@tanstack/react-query';

/**
 * Supported cache keys for invalidation
 */
export type CacheKey =
  | 'accounts'
  | 'expenses'
  | 'incomes'
  | 'projections'
  | 'users';

/**
 * Cache invalidation dependency rules
 * When a key is invalidated, all its dependencies are automatically invalidated
 */
const INVALIDATION_DEPENDENCIES: Record<CacheKey, CacheKey[]> = {
  // Core entities - no dependencies
  users: [],

  // Accounts changes affect projections
  accounts: ['projections'],

  // Expenses changes affect accounts (which auto-invalidates projections)
  expenses: ['accounts'],

  // Incomes changes affect accounts (which auto-invalidates projections)
  incomes: ['accounts'],

  // Projections is a leaf node - no dependencies
  projections: [],
};

/**
 * Get all cache keys that should be invalidated including dependencies
 * Uses breadth-first traversal to handle nested dependencies
 */
function getAllKeysToInvalidate(keys: CacheKey[]): CacheKey[] {
  const visited = new Set<CacheKey>();
  const queue = [...keys];

  while (queue.length > 0) {
    const currentKey = queue.shift()!;

    if (visited.has(currentKey)) {
      continue;
    }

    visited.add(currentKey);

    // Add dependencies to queue
    const dependencies = INVALIDATION_DEPENDENCIES[currentKey];
    for (const dependency of dependencies) {
      if (!visited.has(dependency)) {
        queue.push(dependency);
      }
    }
  }

  return Array.from(visited);
}

/**
 * Centralized cache invalidation helper with automatic dependency handling
 *
 * USE THIS VERSION: For utility functions, bulk actions, and non-React contexts
 *
 * @param queryClient - React Query client instance
 * @param keys - Cache keys to invalidate (dependencies will be auto-invalidated)
 *
 * @example
 * // In utility functions (bulk actions, etc.)
 * import { invalidateCache } from '@/lib';
 * invalidateCache(queryClient, ['expenses']); // Invalidates expenses + accounts + projections
 *
 * // Invalidates accounts + projections
 * invalidateCache(queryClient, ['accounts']);
 *
 * // Invalidates multiple keys + their dependencies
 * invalidateCache(queryClient, ['expenses', 'incomes']);
 */
export function invalidateCache(
  queryClient: QueryClient,
  keys: CacheKey[],
): void {
  const allKeysToInvalidate = getAllKeysToInvalidate(keys);

  // Invalidate all required cache keys
  for (const key of allKeysToInvalidate) {
    queryClient.invalidateQueries({ queryKey: [key] });
  }
}

/**
 * Hook-friendly version that returns a bound invalidation function
 *
 * USE THIS VERSION: For React hooks and components only
 *
 * @param queryClient - React Query client instance
 * @returns Function to invalidate cache with dependency handling
 *
 * @example
 * // In React hooks/components
 * const invalidateCache = useCacheInvalidation(queryClient);
 * // Later...
 * invalidateCache(['expenses']); // Auto-invalidates accounts + projections
 */
export function useCacheInvalidation(queryClient: QueryClient) {
  return (keys: CacheKey[]) => invalidateCache(queryClient, keys);
}
