export abstract class FailResultBase {
  constructor(
    public type: string, // An error type identifier, such as "IO", "Api", etc.
    public code: string, // An error specific code, such as "Conflict", "Validation", etc.
    public description: string,
  ) {}
}
