import { FailResultBase } from '@/lib';

// These const objects exist at runtime
const ErrorType = {
  Api: 'Api',
  Network: 'Network',
  Unexpected: 'Unexpected',
} as const;

const ErrorCode = {
  Authentication: 'Authentication Error',
  Validation: 'Validation Error',
  NotFound: 'Not Found',
  Conflict: 'Conflict Error',
  Network: 'Network Error',
  Unexpected: 'Unexpected Error',
  Forbidden: 'Forbidden Error',
} as const;

// These type declarations only exist at compile time
// They extract the literal types from the const objects above
// They can have the same names as the consts because they're in the type namespace
type ErrorType = (typeof ErrorType)[keyof typeof ErrorType];
type ErrorCode = (typeof ErrorCode)[keyof typeof ErrorCode];

abstract class ApiError extends FailResultBase {
  constructor(code: ErrorCode, description: string) {
    super(ErrorType.Api, code, description);
  }
}

class AuthenticationError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Authentication, description);
  }
}

class ValidationError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Validation, description);
  }
}

class NotFoundError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.NotFound, description);
  }
}

class ConflictError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Conflict, description);
  }
}

class UnexpectedError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Unexpected, description);
  }
}

class ForbiddenError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Forbidden, description);
  }
}

class NetworkError extends FailResultBase {
  constructor(description: string) {
    super(ErrorType.Network, ErrorCode.Network, description);
  }
}

export type { ErrorCode, ErrorType };

export {
  ApiError,
  AuthenticationError,
  ConflictError,
  ForbiddenError,
  NetworkError,
  NotFoundError,
  UnexpectedError,
  ValidationError,
};
