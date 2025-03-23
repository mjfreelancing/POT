export abstract class FailResultBase {
  constructor(
    public type?: string, // An optional error type identifier, such as "IO", "Api", etc.
    public code?: string, // An optional code to provide granularity for the failure type.
    public description?: string,
  ) {}
}
