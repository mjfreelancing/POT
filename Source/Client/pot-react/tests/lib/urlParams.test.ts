import { describe, expect, test } from 'vitest';

import { listFilterQuery } from '@/lib';

describe('listFilterQuery', () => {
  test.each([
    [
      'returns an empty query when search has no list filter params',
      '?duplicate=5',
      {},
      '',
    ],
    ['returns an empty query for an empty search', '', {}, ''],
    ['keeps the accountId list filter', '?accountId=3', {}, '?accountId=3'],
    [
      'drops a transient duplicate param that sits beside accountId',
      '?accountId=3&duplicate=5',
      {},
      '?accountId=3',
    ],
    [
      'drops unknown non-list params rather than echoing them',
      '?accountId=3&foo=bar',
      {},
      '?accountId=3',
    ],
    [
      'adds extra route params when no list filter is active',
      '',
      { duplicate: '5' },
      '?duplicate=5',
    ],
    [
      'merges extra route params with the forwarded accountId filter',
      '?accountId=3',
      { duplicate: '5' },
      '?accountId=3&duplicate=5',
    ],
  ])('%s', (_name, search, extra, expected) => {
    expect(listFilterQuery(search, extra)).toBe(expected);
  });
});
