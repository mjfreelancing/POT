// Example to simulate a slow connection
// return withDelay(() => accountsApi.getAll(signal));

import { isDevelopment } from './utils';

const sleep = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export const withDelay = async <T>(
  fn: () => Promise<T>,
  delay = 3000,
): Promise<T> => {
  if (!isDevelopment()) {
    throw new Error('withDelay should only be used in development mode');
  }

  await sleep(delay);

  return await fn();
};
