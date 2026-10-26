import { readJsonBody } from '../apiHelpers';

type ApiErrorResponse = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  // The validation messages, keyed by the field they belong to. Only a 400 carries them.
  errors?: Record<string, string[]>;
  // The framework's trace identifier: the W3C Activity id when tracing is active, otherwise the
  // request identifier.
  traceId?: string;
  // The server's correlation id: the caller's X-Correlation-Id when supplied.
  correlationId?: string;
  instance?: string;
};

const isRecord = (value: unknown): value is Record<string, unknown> => {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
};

// A member is read only when it is text: any other type is a body this client cannot interpret, and
// a blank one would leave a caller with an empty message.
const readText = (value: unknown): string | undefined => {
  return typeof value === 'string' && value.trim().length > 0
    ? value
    : undefined;
};

const readMessages = (value: unknown): string[] => {
  if (!Array.isArray(value)) {
    return [];
  }

  return value.filter(
    (message): message is string =>
      typeof message === 'string' && message.trim().length > 0,
  );
};

const readErrors = (value: unknown): Record<string, string[]> | undefined => {
  if (!isRecord(value)) {
    return undefined;
  }

  const fields = Object.entries(value)
    .map(([field, messages]) => [field, readMessages(messages)] as const)
    .filter(([, messages]) => messages.length > 0);

  return fields.length > 0 ? Object.fromEntries(fields) : undefined;
};

// A body is reduced to the members it can be trusted to carry: an unknown member, a member of the
// wrong type and a blank member are all dropped, so every getter below stays total.
const toApiErrorResponse = (value: unknown): ApiErrorResponse => {
  const error: ApiErrorResponse = {};

  if (!isRecord(value)) {
    return error;
  }

  const textMembers = [
    'type',
    'title',
    'detail',
    'traceId',
    'correlationId',
    'instance',
  ] as const;

  for (const member of textMembers) {
    const text = readText(value[member]);

    if (text !== undefined) {
      error[member] = text;
    }
  }

  const errors = readErrors(value.errors);

  if (errors !== undefined) {
    error.errors = errors;
  }

  if (typeof value.status === 'number') {
    error.status = value.status;
  }

  return error;
};

/**
 * Reads any error body into the problem shape
 *
 * A body this client cannot interpret - a page written by a reverse proxy, an empty body, JSON that
 * is not problem shaped - is treated as absent, so it can never become a message.
 */
async function readApiErrorResponse(data: unknown): Promise<ApiErrorResponse> {
  return toApiErrorResponse(await readJsonBody(data));
}

const getNotFoundMessage = (_: ApiErrorResponse): string => {
  return 'The requested resource was not found';
};

const getMethodNotAllowedMessage = (_: ApiErrorResponse): string => {
  return 'The requested method is not allowed for this resource';
};

const getAuthenticationMessage = (_: ApiErrorResponse): string => {
  return 'Invalid username or password';
};

const getForbiddenMessage = (_: ApiErrorResponse): string => {
  return 'You do not have permission to access this resource';
};

const getRateLimitedMessage = (_: ApiErrorResponse): string => {
  return 'Too many requests. Please wait a moment and try again.';
};

// The server names the conflict category in 'type', so a stale update is recognised without inspecting
// a field name.
const staleUpdateProblemType = 'urn:pot:problem:stale-entity-tag';

const getConflictMessage = (error: ApiErrorResponse): string => {
  // A stale tag is the one conflict whose meaning the server's message does not convey on its own: the
  // caller has to know it must refresh before retrying.
  if (error.type === staleUpdateProblemType) {
    return 'A conflicting update has been performed by another user. Refresh and try again.';
  }

  // Every other conflict states what happened in 'detail', written by the server for the user.
  return error.detail ?? 'A conflict error occurred';
};

const getBadRequestMessage = (error: ApiErrorResponse): string => {
  // The caller needs every message, field and general alike, whatever field the server keyed it by.
  const messages = Object.values(error.errors ?? {}).flat();

  if (messages.length > 0) {
    return messages.join('\n');
  }

  return error.detail ?? error.title ?? 'The request was not valid.';
};

const getServerErrorMessage = (error: ApiErrorResponse): string => {
  return (
    error.detail ??
    error.title ??
    'The server encountered an error. Please try again.'
  );
};

// A 502, 503 or 504 is answered by whatever sits in front of the application, so a body with no
// message of its own means the application was never reached rather than that it failed.
const getUnreachableMessage = (error: ApiErrorResponse): string => {
  return error.detail ?? error.title ?? 'Unable to connect to the server.';
};

const getErrorTitle = (error: ApiErrorResponse): string => {
  return error.detail ?? error.title ?? 'An unknown error occurred';
};

export {
  getAuthenticationMessage,
  getBadRequestMessage,
  getConflictMessage,
  getErrorTitle,
  getForbiddenMessage,
  getMethodNotAllowedMessage,
  getNotFoundMessage,
  getRateLimitedMessage,
  getServerErrorMessage,
  getUnreachableMessage,
  readApiErrorResponse,
};
export type { ApiErrorResponse };
