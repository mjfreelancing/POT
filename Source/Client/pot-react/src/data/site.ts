import { z } from 'zod';

import { EtagSchema, IdentitySchema } from './identity';

const BaseSiteSchema = z.object({
  name: z.string().min(1, 'Site name is required'),
  description: z.string().optional(),
});

const SiteSchema = BaseSiteSchema.extend({
  ...IdentitySchema.shape,
});

const EditSiteSchema = BaseSiteSchema.extend({
  ...EtagSchema.shape,
});

type Site = z.infer<typeof SiteSchema>;
type EditSite = z.infer<typeof EditSiteSchema>;

export { BaseSiteSchema, EditSiteSchema, SiteSchema };
export type { EditSite, Site };
