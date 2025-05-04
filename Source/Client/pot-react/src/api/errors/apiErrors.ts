import { FailResultBase } from '@/lib/result/failResultBase';

export const ErrorType = {
  Api: 'Api',
  Unexpected: 'Unexpected',
} as const;

export const ErrorCode = {
  Validation: 'Validation',
  Conflict: 'Conflict',
  Unexpected: 'Unexpected',
} as const;

export type ErrorType = (typeof ErrorType)[keyof typeof ErrorType];
export type ErrorCode = (typeof ErrorCode)[keyof typeof ErrorCode];

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
