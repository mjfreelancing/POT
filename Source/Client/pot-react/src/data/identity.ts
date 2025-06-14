import { z } from 'zod';

const IdentitySchema = z.object({
  rowId: z.string(),
  etag: z.bigint(),
});

type Identity = z.infer<typeof IdentitySchema>;

export { IdentitySchema };
export type { Identity };
