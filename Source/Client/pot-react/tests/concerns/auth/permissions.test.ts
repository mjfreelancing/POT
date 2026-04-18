import { describe, expect, test } from 'vitest';

import { ALL_PERMISSIONS, PERMISSIONS } from '@/concerns/auth/permissions';

describe('permissions concern', () => {
  test('exports expected permission values', () => {
    expect(PERMISSIONS).toEqual({
      siteManage: 'site:manage',
      siteView: 'site:view',
      userManage: 'user:manage',
      userView: 'user:view',
      accountManage: 'account:manage',
      accountView: 'account:view',
      expenseManage: 'expense:manage',
      expenseView: 'expense:view',
      incomeManage: 'income:manage',
      incomeView: 'income:view',
      maintenanceExport: 'maintenance:export',
      maintenanceImport: 'maintenance:import',
      platformManage: 'platform:manage',
    });
  });

  test('ALL_PERMISSIONS matches PERMISSIONS values', () => {
    expect(ALL_PERMISSIONS).toEqual(Object.values(PERMISSIONS));
  });

  test('ALL_PERMISSIONS contains unique values', () => {
    const uniquePermissions = new Set(ALL_PERMISSIONS);

    expect(uniquePermissions.size).toBe(ALL_PERMISSIONS.length);
  });
});
