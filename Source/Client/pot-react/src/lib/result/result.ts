import { FailResultBase } from './failResultBase';

export class SuccessResult<TSuccess> {
  public readonly success = true;
  constructor(public value: TSuccess) {}
}

export class FailResult<TFail extends FailResultBase> {
  public readonly success = false;
  constructor(public error: TFail) {}
}

export type Result<TSuccess, TFail extends FailResultBase> =
  | SuccessResult<TSuccess>
  | FailResult<TFail>;
