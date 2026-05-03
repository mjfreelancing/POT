import { beforeEach, describe, expect, test } from 'vitest';

import { migrateStorageEntries } from '@/concerns/storage/storageMigration';

describe('migrateStorageEntries', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  test('migrates value from old key to new key when old key exists and new key does not', () => {
    localStorage.setItem('pot-dashboard', JSON.stringify({ accountsOpen: true }));

    migrateStorageEntries([
      { oldKey: 'pot-dashboard', newKey: 'pot:dev:user:u1:dashboard', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot:dev:user:u1:dashboard')).toBe(
      JSON.stringify({ accountsOpen: true }),
    );
    expect(localStorage.getItem('pot-dashboard')).toBeNull();
  });

  test('removes old key when both old and new keys already exist, keeping new key data', () => {
    localStorage.setItem('pot-dashboard', JSON.stringify({ accountsOpen: true }));
    localStorage.setItem('pot:dev:user:u1:dashboard', JSON.stringify({ accountsOpen: false }));

    migrateStorageEntries([
      { oldKey: 'pot-dashboard', newKey: 'pot:dev:user:u1:dashboard', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot:dev:user:u1:dashboard')).toBe(
      JSON.stringify({ accountsOpen: false }),
    );
    expect(localStorage.getItem('pot-dashboard')).toBeNull();
  });

  test('does nothing when old key does not exist', () => {
    migrateStorageEntries([
      { oldKey: 'pot-dashboard', newKey: 'pot:dev:user:u1:dashboard', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot:dev:user:u1:dashboard')).toBeNull();
  });

  test('processes all entries in the list', () => {
    localStorage.setItem('pot-accounts', '"accounts-data"');
    localStorage.setItem('pot-incomes', '"incomes-data"');

    migrateStorageEntries([
      { oldKey: 'pot-accounts', newKey: 'pot:dev:user:u1:accounts', storage: localStorage },
      { oldKey: 'pot-incomes', newKey: 'pot:dev:user:u1:incomes', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot:dev:user:u1:accounts')).toBe('"accounts-data"');
    expect(localStorage.getItem('pot:dev:user:u1:incomes')).toBe('"incomes-data"');
    expect(localStorage.getItem('pot-accounts')).toBeNull();
    expect(localStorage.getItem('pot-incomes')).toBeNull();
  });

  test('processes entries independently so an absent old key does not block other entries', () => {
    localStorage.setItem('pot-accounts', '"accounts-data"');

    migrateStorageEntries([
      { oldKey: 'pot-missing', newKey: 'pot:dev:user:u1:missing', storage: localStorage },
      { oldKey: 'pot-accounts', newKey: 'pot:dev:user:u1:accounts', storage: localStorage },
    ]);

    expect(localStorage.getItem('pot:dev:user:u1:missing')).toBeNull();
    expect(localStorage.getItem('pot:dev:user:u1:accounts')).toBe('"accounts-data"');
  });
});
