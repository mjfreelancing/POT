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

export class ValidationError extends FailResultBase {
  constructor(description: string) {
    super(ErrorType.Api, ErrorCode.Validation, description);
  }
}

export class ConflictError extends FailResultBase {
  constructor(description: string) {
    super(ErrorType.Api, ErrorCode.Conflict, description);
  }
}

export class UnexpectedError extends FailResultBase {
  constructor(description: string) {
    super(ErrorType.Unexpected, ErrorCode.Unexpected, description);
  }
}
