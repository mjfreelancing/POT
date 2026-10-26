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
  // Check if we actually have error data from the server
  // If the server isn't running, error object will be empty
  const hasErrorData =
    error.detail || error.title || error.type || error.traceId;

  if (!hasErrorData) {
    // Empty error object = server not responding
    return 'Unable to connect to the server.';
  }

  // Server is running and returned actual error data
  if (error.detail) {
    return error.detail;
  }

  if (error.title) {
    return error.title;
  }

  // Fallback for malformed server errors
  return 'The server encountered an error. Please try again.';
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
};
export type { ApiErrorResponse };
