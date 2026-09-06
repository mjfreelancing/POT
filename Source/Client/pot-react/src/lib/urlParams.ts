/**
 * Query params that remain meaningful when navigating between a list route and
 * its edit/create sub-routes. Only these are forwarded; any other param present
 * in the source search (for example the transient `duplicate` param on a create
 * route) is dropped rather than echoed, so navigation stays deterministic and
 * stale sub-route params can never leak back onto the list URL.
 */
const LIST_FILTER_PARAMS = ['accountId'] as const;

/**
 * Returns the list filter params found in `search` (a `location.search` string)
 * as a query suffix ('' or '?accountId=…'), optionally merged with `extra`
 * params that belong to the target route.
 *
 * Sub-routes never echo the whole search string - only the declared list filter
 * params are forwarded - so there is no need to remember to strip transient
 * params such as `duplicate`.
 *
 * @example
 * listFilterQuery('')                                    // => ''
 * listFilterQuery('?accountId=3')                        // => '?accountId=3'
 * listFilterQuery('?duplicate=5')                        // => ''
 * listFilterQuery('?duplicate=5&accountId=3')            // => '?accountId=3'
 * listFilterQuery('?accountId=3', { duplicate: '5' })    // => '?accountId=3&duplicate=5'
 */
export function listFilterQuery(
  search: string,
  extra: Record<string, string> = {},
): string {
  const source = new URLSearchParams(search);
  const query = new URLSearchParams();

  for (const param of LIST_FILTER_PARAMS) {
    const value = source.get(param);
    if (value) {
      query.set(param, value);
    }
  }

  for (const [param, value] of Object.entries(extra)) {
    query.set(param, value);
  }

  const serialized = query.toString();

  return serialized ? `?${serialized}` : '';
}
