import { beforeEach, describe, expect, test } from 'vitest';

import {
  setSessionNotice,
  takeSessionNotice,
} from '@/features/auth/sessionNotice';

describe('session notice', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  test('takes the recorded message once', () => {
    setSessionNotice('Your account has been deleted.');

    expect(takeSessionNotice()).toBe('Your account has been deleted.');
    expect(takeSessionNotice()).toBeUndefined();
  });

  test('returns undefined when nothing was recorded', () => {
    expect(takeSessionNotice()).toBeUndefined();
  });
});
