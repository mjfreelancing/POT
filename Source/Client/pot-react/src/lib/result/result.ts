import { FailResultBase } from './failResultBase';

export class Success<TSuccess> {
  public readonly success = true;
  constructor(public value: TSuccess) {}
}

export class Failure<TFail extends FailResultBase> {
  public readonly success = false;
  constructor(public error: TFail) {}
}

export type Result<TSuccess, TFail extends FailResultBase> =
  | Success<TSuccess>
  | Failure<TFail>;
