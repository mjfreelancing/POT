export abstract class FailResultBase {
  public type?: string;
  public code?: string;
  public description?: string;

  constructor(type?: string, code?: string, description?: string) {
    this.type = type;
    this.code = code;
    this.description = description;
  }
}
