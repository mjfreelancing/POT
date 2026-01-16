type ApiErrorDetail = {
  propertyName: string;
  errorCode: string;
  attemptedValue: string;
  errorMessage: string;
};

type ApiErrorResponse = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: ApiErrorDetail[];
  traceId?: string;
  correlationId?: string;
  instance?: string;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getNotFoundMessage = (_error: ApiErrorResponse): string => {
  return 'The requested resource was not found';
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getMethodNotAllowedMessage = (_error: ApiErrorResponse): string => {
  return 'The requested method is not allowed for this resource';
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getAuthenticationMessage = (_error: ApiErrorResponse): string => {
  return 'Invalid username or password';
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getForbiddenMessage = (_error: ApiErrorResponse): string => {
  return 'You do not have permission to access this resource';
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getRateLimitedMessage = (_error: ApiErrorResponse): string => {
  return 'Too many requests. Please wait a moment and try again.';
};

const getConflictMessage = (error: ApiErrorResponse): string => {
  if (error.errors && error.errors.length > 0) {
    return error.errors
      .map(err => {
        if (err.propertyName.toLowerCase() === 'etag') {
          return 'A conflicting update has been performed by another user. Refresh and try again.';
        }

        return `The '${err.propertyName}' conflicts with another record that has the same value '${err.attemptedValue}'`;
      })
      .join('\n');
  }

  return error.detail ?? 'A conflict error occurred';
};

const getValidationMessage = (error: ApiErrorResponse): string => {
  if (error.errors && error.errors.length > 0) {
    return error.errors.map(err => err.errorMessage).join('\n');
  }

  return error.detail ?? 'A validation error occurred';
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

export type { ApiErrorDetail, ApiErrorResponse };
export {
  getAuthenticationMessage,
  getConflictMessage,
  getErrorTitle,
  getForbiddenMessage,
  getMethodNotAllowedMessage,
  getNotFoundMessage,
  getRateLimitedMessage,
  getServerErrorMessage,
  getValidationMessage,
};
