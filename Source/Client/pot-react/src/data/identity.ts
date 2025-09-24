import { z } from 'zod';

const RowIdSchema = z.object({
  rowId: z.string(),
});

const EtagSchema = z.object({
  etag: z.bigint(),
});

const IdentitySchema = RowIdSchema.merge(EtagSchema);

type RowId = z.infer<typeof RowIdSchema>;
type Etag = z.infer<typeof EtagSchema>;
type Identity = z.infer<typeof IdentitySchema>;

export { EtagSchema, IdentitySchema, RowIdSchema };
export type { Etag, Identity, RowId };
