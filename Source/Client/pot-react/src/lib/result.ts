abstract class FailResultBase {
  constructor(
    public type: string, // An error type identifier, such as "IO", "Api", etc.
    public code: string, // An error specific code, such as "Conflict", "Validation", etc.
    public description: string,
  ) {}
}

class SuccessResult<TSuccess> {
  public readonly success = true;
  constructor(public value: TSuccess) {}
}

class FailResult<TFail extends FailResultBase> {
  public readonly success = false;
  constructor(public error: TFail) {}
}

type Result<TSuccess, TFail extends FailResultBase> =
  SuccessResult<TSuccess> | FailResult<TFail>;

export { FailResult, FailResultBase, SuccessResult };
export type { Result };
