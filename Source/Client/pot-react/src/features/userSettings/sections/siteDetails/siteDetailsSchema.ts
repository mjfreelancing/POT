import * as z from 'zod';

const siteDetailsSchema = z.object({
  name: z.string().min(1, 'Site name is required'),
  description: z.string().optional(),
});

export type SiteDetailsFields = z.infer<typeof siteDetailsSchema>;
export { siteDetailsSchema };
