export type ApiErrorDetail = {
  propertyName: string;
  errorCode: string;
  attemptedValue: string;
  errorMessage: string;
};

export type ApiErrorResponse = {
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
export const getNotFoundMessage = (_error: ApiErrorResponse): string => {
  return 'The requested resource was not found';
};

export const getConflictMessage = (error: ApiErrorResponse): string => {
  if (error.errors?.length) {
    const firstError = error.errors[0];
    return `The '${firstError.propertyName}' conflicts with another record that has the same value '${firstError.attemptedValue}'.`;
  }

  return error.detail ?? 'A conflict error occurred';
};

export const getValidationMessage = (error: ApiErrorResponse): string => {
  if (error.errors?.length) {
    return error.errors[0].errorMessage;
  }

  return error.detail ?? 'A validation error occurred';
};

// TODO: Check what happens when the error is a 500
export const getErrorMessage = (error: ApiErrorResponse): string => {
  return error.title ?? 'An unknown error occurred';
};
