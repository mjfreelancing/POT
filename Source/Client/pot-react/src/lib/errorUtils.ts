// Workaround for: Catch clause variable type annotation must be 'any' or 'unknown' if specified.
// ie., cannot use catch(error: Error) {}
function getErrorMessage(error: unknown) {
  if (error instanceof Error) return error.message;
  return String(error);
}

export { getErrorMessage };
