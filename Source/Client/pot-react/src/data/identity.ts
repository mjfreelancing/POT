import { z } from 'zod';

export const IdentitySchema = z.object({
  rowId: z.string(),
  eTag: z.bigint(),
});

export type Identity = z.infer<typeof IdentitySchema>;
