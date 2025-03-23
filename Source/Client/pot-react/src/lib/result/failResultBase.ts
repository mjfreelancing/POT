export abstract class FailResultBase {
  constructor(
    public type?: string,
    public code?: string,
    public description?: string,
  ) {}
}
