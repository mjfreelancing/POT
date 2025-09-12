/**
 * Logging utility for consistent log formatting across the application
 */
const logger = {
  info: (component: string, message: string, ...args: unknown[]) => {
    console.info(`[${component}] ${message}`, ...(args.length ? args : []));
  },

  warn: (component: string, message: string, ...args: unknown[]) => {
    console.warn(`[${component}] ${message}`, ...(args.length ? args : []));
  },

  error: (component: string, message: string, error?: unknown) => {
    console.error(`[${component}] ${message}`, error || '');
  },
};

export { logger };
