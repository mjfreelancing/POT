import { z } from 'zod';

export const AccountSchema = z.object({
  rowId: z.string(),
  eTag: z.bigint(),
  bsb: z.string(),
  number: z.string(),
  description: z.string(),
  balance: z.number(),
  reserved: z.number(),
  allocated: z.number(),
  dailyAccrual: z.number(),
  available: z.number(),
});

export type Account = z.infer<typeof AccountSchema>;
