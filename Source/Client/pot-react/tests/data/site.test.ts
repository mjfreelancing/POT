import { describe, expect, test } from 'vitest';

import { BaseSiteSchema, EditSiteSchema, SiteSchema } from '@/data/site';

describe('site schemas', () => {
  test('parses valid site payloads', () => {
    expect(
      BaseSiteSchema.parse({
        name: 'HQ',
        description: 'Main office',
      }),
    ).toBeTruthy();

    expect(
      EditSiteSchema.parse({
        name: 'HQ',
        description: 'Main office',
        etag: 2n,
      }),
    ).toBeTruthy();

    expect(
      SiteSchema.parse({
        rowId: 'site-1',
        etag: 3n,
        name: 'HQ',
        description: 'Main office',
      }),
    ).toBeTruthy();
  });

  test('rejects invalid site payloads', () => {
    expect(() => BaseSiteSchema.parse({ name: '' })).toThrow();
    expect(() => EditSiteSchema.parse({ name: 'HQ', etag: 2 })).toThrow();
    expect(() => SiteSchema.parse({ name: 'HQ' })).toThrow();
  });
});
