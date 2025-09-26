import { DisplayError, getErrorMessage, logger } from '@/lib';

type LocalStorageProps<T> = {
  key: string;
  defaultValue?: T;
  onError?: (error: DisplayError) => void;
};

function getDisplayError(error: unknown): DisplayError {
  return {
    title: 'Local Storage Error',
    description: getErrorMessage(error),
  };
}

const useLocalStorage = <T = Record<string, unknown>>({
  key,
  defaultValue,
  onError,
}: LocalStorageProps<T>) => {
  const setItem = (value: T) => {
    try {
      window.localStorage.setItem(key, JSON.stringify(value));
    } catch (error) {
      logger.error(
        'useLocalStorage',
        `Error setting item with key "${key}"`,
        error,
      );

      onError?.(getDisplayError(error));
    }
  };

  const getItem = (): T | undefined => {
    try {
      const item = window.localStorage.getItem(key);

      return item ? JSON.parse(item) : defaultValue;
    } catch (error) {
      logger.error(
        'useLocalStorage',
        `Error getting item with key "${key}"`,
        error,
      );

      onError?.(getDisplayError(error));
      return undefined;
    }
  };

  const removeItem = () => {
    try {
      window.localStorage.removeItem(key);
    } catch (error) {
      logger.error(
        'useLocalStorage',
        `Error removing item with key "${key}"`,
        error,
      );

      onError?.(getDisplayError(error));
    }
  };

  return { setItem, getItem, removeItem };
};

export default useLocalStorage;
export type { LocalStorageProps };
