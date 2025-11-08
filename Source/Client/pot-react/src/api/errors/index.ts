// Barrel file for API errors - centralized exports for cleaner imports

// Error response message generators
export {
  getAuthenticationMessage,
  getConflictMessage,
  getErrorTitle,
  getForbiddenMessage,
  getMethodNotAllowedMessage,
  getNotFoundMessage,
  getRateLimitedMessage,
  getValidationMessage,
} from './apiErrorResponse';

// Error types and classes
export type { ApiError } from './apiErrors';
export {
  AuthenticationError,
  ConflictError,
  ForbiddenError,
  MethodNotAllowedError,
  NetworkError,
  NotFoundError,
  RateLimitedError,
  UnexpectedError,
  ValidationError,
} from './apiErrors';

// Response types
export type { ApiErrorResponse } from './apiErrorResponse';
