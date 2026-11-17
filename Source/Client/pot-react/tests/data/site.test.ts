import { describe, expect, test } from 'vitest';

import {
  BaseSiteSchema,
  DeleteSiteSchema,
  EditSiteSchema,
  SiteSchema,
} from '@/data/site';

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
        etag: 2,
      }),
    ).toBeTruthy();

    expect(
      SiteSchema.parse({
        rowId: 'site-1',
        etag: 3,
        name: 'HQ',
        description: 'Main office',
      }),
    ).toBeTruthy();
  });

  test('rejects invalid site payloads', () => {
    expect(() => BaseSiteSchema.parse({ name: '' })).toThrow();
    expect(() => EditSiteSchema.parse({ name: 'HQ' })).toThrow();
    expect(() => SiteSchema.parse({ name: 'HQ' })).toThrow();
  });

  test('requires the site name, username and password to delete a site', () => {
    expect(
      DeleteSiteSchema.parse({
        siteName: 'HQ',
        username: 'maria',
        password: 'secret',
      }),
    ).toEqual({ siteName: 'HQ', username: 'maria', password: 'secret' });

    expect(() =>
      DeleteSiteSchema.parse({ username: 'maria', password: 'secret' }),
    ).toThrow();
    expect(() =>
      DeleteSiteSchema.parse({ siteName: 'HQ', password: 'secret' }),
    ).toThrow();
    expect(() =>
      DeleteSiteSchema.parse({ siteName: 'HQ', username: 'maria' }),
    ).toThrow();
  });
});
