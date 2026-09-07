type ParseIssue = { path: PropertyKey[]; message: string };

export type SchemaParseResult<T> =
  | { success: true; data: T }
  | { success: false; error: { issues: ParseIssue[] } };

/**
 * Flattens a zod `safeParse` error into stable `path.message` entries so tests
 * can assert the exact fields and messages a schema produces without depending
 * on zod's error-object shape across major versions.
 */
export function flattenIssues<T>(
  result: SchemaParseResult<T>,
): { path: string; message: string }[] {
  if (result.success) {
    return [];
  }

  return result.error.issues.map(issue => ({
    path: issue.path.join('.'),
    message: issue.message,
  }));
}
