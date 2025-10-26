import * as z from 'zod';

const siteSettingsSchema = z.object({
  name: z.string().min(1, 'Site name is required'),
  description: z.string().optional(),
});

export type SiteSettingsFields = z.infer<typeof siteSettingsSchema>;
export { siteSettingsSchema };
