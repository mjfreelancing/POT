// Example to simulate a slow connection
//
// Change this:
//   return axios.get<TResponse>(url, { signal }).then(responseData);
//
// To this:
//   return withDelay(() =>axios.get<TResponse>(url, { signal }).then(responseData));
//
// Or just use a sleep() if that's all you need:
//   await sleep(3000);

import { isDevelopment } from './utils';

const sleep = (ms: number) => {
  if (!isDevelopment()) {
    throw new Error('sleep should only be used in development mode');
  }

  new Promise(resolve => setTimeout(resolve, ms));
};

const withDelay = async <T>(fn: () => Promise<T>, delay = 3000): Promise<T> => {
  if (!isDevelopment()) {
    throw new Error('withDelay should only be used in development mode');
  }

  await sleep(delay);
  return await fn();
};

export { sleep, withDelay };
