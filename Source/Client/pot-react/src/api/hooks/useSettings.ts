import type { Identity, SettingsResponse, UpdateSettingRequest } from '@/data';
import type { FailResultBase, Result } from '@/lib';

import { useGet, usePutWithId } from './useApi';

function useApiGetSettings() {
  const query = useGet<SettingsResponse>('/settings', ['settings']);

  return {
    ...query,
    data: query.data as Result<SettingsResponse, FailResultBase>,
  };
}

function useApiUpdateSetting() {
  const mutation = usePutWithId<Identity, UpdateSettingRequest>(
    settingPath => `/settings/${settingPath}`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
}

export { useApiGetSettings, useApiUpdateSetting };
