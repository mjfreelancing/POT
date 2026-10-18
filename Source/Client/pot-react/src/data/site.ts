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

const DeleteSiteSchema = z.object({
  siteName: z.string(),
  username: z.string(),
  password: z.string(),
});

type Site = z.infer<typeof SiteSchema>;
type EditSite = z.infer<typeof EditSiteSchema>;
type DeleteSite = z.infer<typeof DeleteSiteSchema>;

export { BaseSiteSchema, DeleteSiteSchema, EditSiteSchema, SiteSchema };
export type { DeleteSite, EditSite, Site };
