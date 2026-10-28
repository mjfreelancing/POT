import { expect, test } from '@playwright/test';

import { oneTimeCode, waitForMessage } from '../../helpers/mail';

/**
 * Proves the browser tier can read a message the application sent.
 *
 * The code it needs exists nowhere else: the sign-up service stores only the code's hash, so a suite that cannot
 * read the message cannot complete a sign-up. Reading it from the sink is therefore the whole point of the
 * assertion, not a shortcut.
 */
test.describe('Mail sink', () => {
  test('reads a one-time code from the message the application sent', async ({
    request,
  }) => {
    const suffix = Date.now().toString(36);
    const username = `sink-${suffix}`;
    const email = `${username}@local.test`;

    const sendResponse = await request.post('/api/auth/signup/send', {
      data: { username, email },
    });

    expect(sendResponse.ok()).toBeTruthy();

    const sendBody = (await sendResponse.json()) as {
      referenceCode?: string;
    };

    expect(sendBody.referenceCode).toBeTruthy();

    // The subject carries the message type ("POT - Signup", "POT - Invitation"), which is how a test asks for the
    // message it caused rather than whichever one arrived first.
    const message = await waitForMessage({
      to: email,
      subjectContains: 'Signup',
    });

    const verificationCode = oneTimeCode(message);

    expect(verificationCode).toMatch(/^\d{6}$/);

    const completeResponse = await request.post('/api/auth/signup/complete', {
      data: {
        username,
        referenceCode: sendBody.referenceCode,
        verificationCode,
      },
    });

    const completeBody = (await completeResponse.json()) as {
      status?: string;
    };

    expect(completeResponse.ok()).toBeTruthy();
    expect(completeBody.status).toBe('Success');
  });
});
