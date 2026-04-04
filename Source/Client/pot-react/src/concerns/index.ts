/**
 * Concerns barrel export - cross-cutting functionality used throughout the application
 *
 * Concerns are infrastructure-level utilities and singletons that:
 * - Provide cross-cutting functionality (auth, logging, caching)
 * - Are used by multiple features
 * - Work independently of any specific feature
 * - Are typically singleton instances or global utilities
 */

// Authentication concerns
export type { Permission } from './auth';
export {
  ALL_PERMISSIONS,
  createAuthTokenProvider,
  logoutManager,
  PERMISSIONS,
  tokenProvider,
} from './auth';
export type { TokenProvider } from '@/api/types/auth';

// Cache concerns
export type { CacheKey } from './cache';
export { invalidateCache, useCacheInvalidation } from './cache';

// Logging concerns
export { logger } from './logging';

// PWA concerns
export { registerServiceWorker } from './pwa';
