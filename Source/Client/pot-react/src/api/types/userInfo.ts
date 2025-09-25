import { Identity } from './identity';

type UserInfo = Identity & {
  username: string;
  displayName: string;
  email: string;
  permissions: string[];
};

export type { UserInfo };
