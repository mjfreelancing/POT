import { z } from 'zod';

export const IdentitySchema = z.object({
  rowId: z.string(),
  etag: z.bigint(),
});

export type Identity = z.infer<typeof IdentitySchema>;
