import { useCallback } from 'react';

import useLocalStorage from '@/hooks/useLocalStorage';
import { DisplayError } from '@/lib';

// Type definition for the local storage manager
// - `getProperty`: Retrieves a specific property from the local storage object
// - `setProperty`: Updates a specific property in the local storage object
type LocalStorageManager<T> = {
  getProperty: (property: keyof T) => T[keyof T] | null;
  setProperty: (property: keyof T, value: T[keyof T]) => void;
};

/**
 * Custom hook for managing local storage with type safety.
 * Provides methods to get and set specific properties within a local storage object.
 *
 * @param key - The local storage key to manage.
 * @param onError - Optional error handler for storage-related errors.
 * @param defaultValue - Optional default value for the local storage object.
 * @returns An object with `getProperty` and `setProperty` methods.
 */
function useLocalStorageManager<T>(
  key: string,
  onError?: (error: DisplayError) => void,
  defaultValue: T = {} as T,
): LocalStorageManager<T> {
  const { getItem, setItem } = useLocalStorage<T>({
    key,
    defaultValue,
    onError,
  });

  /**
   * Retrieves the value of a specific property from the local storage object.
   *
   * @param property - The key of the property to retrieve.
   * @returns The value of the property, or `null` if not found.
   */
  const getProperty = useCallback(
    (property: keyof T): T[keyof T] | null => {
      const data = getItem();
      return data ? data[property] : null;
    },
    [getItem],
  );

  /**
   * Updates the value of a specific property in the local storage object.
   *
   * @param property - The key of the property to update.
   * @param value - The new value to set for the property.
   */
  const setProperty = useCallback(
    (property: keyof T, value: T[keyof T]) => {
      const data = getItem() || defaultValue; // Use defaultValue if data is null
      setItem({ ...data, [property]: value }); // Update the specific property
    },
    [getItem, setItem, defaultValue],
  );

  return { getProperty, setProperty };
}

export default useLocalStorageManager;
