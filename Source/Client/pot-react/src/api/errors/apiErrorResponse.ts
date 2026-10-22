type ApiErrorDetail = {
  propertyName?: string | null;
  errorCode: string;
  errorMessage: string;
};

type ApiErrorResponse = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: ApiErrorDetail[];
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

const getConflictMessage = (error: ApiErrorResponse): string => {
  // A stale etag is the one conflict whose meaning the server's message does not convey on its own:
  // the caller needs to know it has to refresh before retrying.
  const isStaleUpdate = error.errors?.some(
    detail => detail.propertyName?.toLowerCase() === 'etag',
  );

  if (isStaleUpdate) {
    return 'A conflicting update has been performed by another user. Refresh and try again.';
  }

  // Every other conflict reports the server's own message, which names the field without echoing
  // the rejected value.
  if (error.detail) {
    return error.detail;
  }

  const fieldConflicts = error.errors?.filter(detail =>
    Boolean(detail.propertyName),
  );

  if (!fieldConflicts || fieldConflicts.length === 0) {
    return 'A conflict error occurred';
  }

  return fieldConflicts.map(detail => detail.errorMessage).join('\n');
};

const getBadRequestMessage = (error: ApiErrorResponse): string => {
  if (error.errors && error.errors.length > 0) {
    return error.errors.map(err => err.errorMessage).join('\n');
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
export type { ApiErrorDetail, ApiErrorResponse };
