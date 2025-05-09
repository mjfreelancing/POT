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
  if (error.errors && error.errors.length > 0) {
    return error.errors
      .map(err => {
        if (err.propertyName === 'Etag') {
          return 'A conflicting update has been performed by another user. Refresh and try again.';
        }

        return `The '${err.propertyName}' conflicts with another record that has the same value '${err.attemptedValue}'`;
      })
      .join('\n');
  }

  return error.detail ?? 'A conflict error occurred';
};

export const getValidationMessage = (error: ApiErrorResponse): string => {
  if (error.errors && error.errors.length > 0) {
    return error.errors.map(err => err.errorMessage).join('\n');
  }

  return error.detail ?? 'A validation error occurred';
};

export const getErrorTitle = (error: ApiErrorResponse): string => {
  return error.title ?? 'An unknown error occurred';
};
