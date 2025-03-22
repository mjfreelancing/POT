/**
 * Represents an error when an operation is invalid in the current state.
 */
export class InvalidOperationError extends Error {
  constructor(
    message: string = 'The operation is invalid in the current state.',
  ) {
    super(message);
    this.name = new.target.name;
  }
}
