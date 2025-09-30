// Workaround for: Catch clause variable type annotation.
// ie., cannot use catch(error: Error) {}
// since error is always unknown
function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}

export { getErrorMessage };
