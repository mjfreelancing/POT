// Barrel file for API errors - centralized exports for cleaner imports

// Error response message generators
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
} from './apiErrorResponse';

// Error types and classes
export type { ApiError } from './apiErrors';
export {
  AuthenticationError,
  BadRequestError,
  ConflictError,
  ForbiddenError,
  MethodNotAllowedError,
  NetworkError,
  NotFoundError,
  RateLimitedError,
  ServerError,
  UnexpectedError,
} from './apiErrors';

// Response types
export type { ApiErrorResponse } from './apiErrorResponse';
