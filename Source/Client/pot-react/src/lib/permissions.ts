/**
 * Permission values object - serves as the single source of truth for all permissions.
 * These must match the permission strings defined in Pot.Shared.Enumerations.Permission.cs
 */
const PERMISSIONS = {
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
} as const;

/**
 * Type-safe permission strings automatically derived from PERMISSIONS object values
 */
type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];

/**
 * All available permissions as a readonly array - automatically generated from PERMISSIONS object
 */
const ALL_PERMISSIONS = Object.values(PERMISSIONS);

export type { Permission };
export { ALL_PERMISSIONS, PERMISSIONS };
