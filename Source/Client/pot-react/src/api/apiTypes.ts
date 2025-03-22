/**
 * Represents HTTP headers as key-value pairs.
 * Each header can have a single value (string) or multiple values (string[]).
 */
export type ApiHeaders = Record<string, string | string[]>;

/**
 * Represents a standard API response structure.
 */
export type ApiResponse = {
  /** HTTP status code of the response. */
  statusCode: number;

  /** Optional content payload of the response. */
  content?: string;

  /** Optioinal HTTP headers included in the response. */
  headers?: ApiHeaders;
};
