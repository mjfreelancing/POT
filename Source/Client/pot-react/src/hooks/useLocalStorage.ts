import { DisplayError,getErrorMessage } from '@/lib';

type LocalStorageProps<T> = {
  key: string;
  initialValue?: T;
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
  initialValue,
  onError,
}: LocalStorageProps<T>) => {
  const setItem = (value: T) => {
    try {
      window.localStorage.setItem(key, JSON.stringify(value));
    } catch (error) {
      console.error(
        `Error setting localStorage item with key "${key}":`,
        error,
      );

      onError?.(getDisplayError(error));
    }
  };

  const getItem = (): T | undefined => {
    try {
      const item = window.localStorage.getItem(key);

      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      console.error(
        `Error getting localStorage item with key "${key}":`,
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
      console.error(
        `Error removing localStorage item with key "${key}":`,
        error,
      );

      onError?.(getDisplayError(error));
    }
  };

  return { setItem, getItem, removeItem };
};

export default useLocalStorage;
export type { LocalStorageProps };
