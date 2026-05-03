import { beforeEach, describe, expect, test } from 'vitest';

import { purgeLegacyStorageKeys } from '@/concerns/storage/storageMigration';

describe('purgeLegacyStorageKeys', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  test('removes the key from storage when it exists', () => {
    localStorage.setItem(
      'pot-dashboard',
      JSON.stringify({ accountsOpen: true }),
    );

    purgeLegacyStorageKeys([{ key: 'pot-dashboard', storage: localStorage }]);

    expect(localStorage.getItem('pot-dashboard')).toBeNull();
  });

  test('does nothing when the key does not exist', () => {
    purgeLegacyStorageKeys([{ key: 'pot-dashboard', storage: localStorage }]);

    expect(localStorage.getItem('pot-dashboard')).toBeNull();
  });

  test('removes all keys in the list', () => {
    localStorage.setItem('pot-accounts', '"accounts-data"');
    localStorage.setItem('pot-incomes', '"incomes-data"');

    purgeLegacyStorageKeys([
      { key: 'pot-accounts', storage: localStorage },
      { key: 'pot-incomes', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot-accounts')).toBeNull();
    expect(localStorage.getItem('pot-incomes')).toBeNull();
  });

  test('processes keys independently so an absent key does not block removal of others', () => {
    localStorage.setItem('pot-accounts', '"accounts-data"');

    purgeLegacyStorageKeys([
      { key: 'pot-missing', storage: localStorage },
      { key: 'pot-accounts', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot-missing')).toBeNull();
    expect(localStorage.getItem('pot-accounts')).toBeNull();
  });
});
