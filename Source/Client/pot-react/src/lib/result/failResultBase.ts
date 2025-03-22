/**
 * Represents the base class for failure results.
 */
export abstract class FailResultBase {
  /** Identifies the type of error. */
  public type?: string;

  /** Error code representing the specific failure. */
  public code?: string;

  /** A human-readable description of the error. */
  public description?: string;

  /**
   * Creates an instance of FailResultBase.
   * @param type - The type of error.
   * @param code - The specific error code.
   * @param description - A human-readable description of the error.
   */
  constructor(type?: string, code?: string, description?: string) {
    this.type = type;
    this.code = code;
    this.description = description;
  }
}
