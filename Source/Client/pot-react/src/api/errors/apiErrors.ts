import { FailResultBase } from '@/lib/result/failResultBase';

// These const objects exist at runtime
export const ErrorType = {
  Api: 'Api',
  Network: 'Network',
  Unexpected: 'Unexpected',
} as const;

export const ErrorCode = {
  Validation: 'Validation Error',
  Conflict: 'Conflict Error',
  Network: 'Network Error',
  Unexpected: 'Unexpected Error',
} as const;

// These type declarations only exist at compile time
// They extract the literal types from the const objects above
// They can have the same names as the consts because they're in the type namespace
type ErrorType = (typeof ErrorType)[keyof typeof ErrorType];
type ErrorCode = (typeof ErrorCode)[keyof typeof ErrorCode];

export abstract class ApiError extends FailResultBase {
  constructor(code: ErrorCode, description: string) {
    super(ErrorType.Api, code, description);
  }
}

export class ValidationError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Validation, description);
  }
}

export class ConflictError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Conflict, description);
  }
}

export class UnexpectedError extends ApiError {
  constructor(description: string) {
    super(ErrorCode.Unexpected, description);
  }
}

export class NetworkError extends FailResultBase {
  constructor(description: string) {
    super(ErrorType.Network, ErrorCode.Network, description);
  }
}
