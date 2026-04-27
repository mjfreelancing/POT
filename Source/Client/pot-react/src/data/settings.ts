import { z } from 'zod';

const SettingValueSchema = z.union([z.boolean(), z.number(), z.string()]);

const SettingSchema = z.object({
  rowId: z.string().optional(),
  etag: z.bigint().optional(),
  category: z.string(),
  key: z.string(),
  value: SettingValueSchema,
  description: z.string(),
});

const SettingsResponseSchema = z.object({
  settings: z.array(SettingSchema),
});

const UpdateSettingRequestSchema = z.object({
  value: z.string(),
  etag: z.bigint().nullable().optional(),
});

type SettingValue = z.infer<typeof SettingValueSchema>;
type Setting = z.infer<typeof SettingSchema>;
type SettingsResponse = z.infer<typeof SettingsResponseSchema>;
type UpdateSettingRequest = z.infer<typeof UpdateSettingRequestSchema>;

export {
  SettingSchema,
  SettingsResponseSchema,
  SettingValueSchema,
  UpdateSettingRequestSchema,
};
export type { Setting, SettingsResponse, SettingValue, UpdateSettingRequest };
