import { mailSinkUrl } from './mailSink';

/**
 * One message the application sent, as the sink stored it.
 *
 * `html` carries everything the templates render; `text` is only populated when a template supplies plain text, so
 * assertions read `html`.
 */
export type SinkMessage = {
  id: string;
  time: string;
  to: string[];
  subject: string;
  html: string;
  text: string;
};

type SinkEmail = {
  id?: string;
  time?: string;
  to?: { address?: string }[];
  subject?: string;
  html?: string;
  text?: string;
};

const defaultTimeoutMs = 15_000;
const pollIntervalMs = 200;

// The templates put a secret in a monospaced block: the sign-up code and the temporary password in the sign-up
// message, and the temporary password alone in the invitation. Reading those blocks is what keeps this helper from
// guessing at prose, and a template that stops emitting them fails the test that needs the secret rather than
// returning something plausible.
const monospacedBlockPattern = /font-family:\s*Consolas[^>]*>([^<]+)</g;

async function readMessages(): Promise<SinkMessage[]> {
  const response = await fetch(`${mailSinkUrl}/email`);

  if (!response.ok) {
    return [];
  }

  const emails = (await response.json()) as SinkEmail[];

  return emails.map(email => ({
    id: email.id ?? '',
    time: email.time ?? '',
    to: (email.to ?? [])
      .map(recipient => recipient.address ?? '')
      .filter(Boolean),
    subject: email.subject ?? '',
    html: email.html ?? '',
    text: email.text ?? '',
  }));
}

/**
 * Waits for a message addressed to a recipient, optionally narrowed by its subject.
 *
 * The sink is the only place a message exists, so waiting is the natural shape: a test does not have to know how
 * long the dispatch loop takes, and a message that never arrives fails with what did.
 */
export async function waitForMessage(options: {
  to: string;
  subjectContains?: string;
  timeoutMs?: number;
}): Promise<SinkMessage> {
  const timeoutMs = options.timeoutMs ?? defaultTimeoutMs;
  const deadline = Date.now() + timeoutMs;

  while (Date.now() < deadline) {
    const messages = await readMessages();

    const match = messages.find(
      message =>
        message.to.some(
          recipient => recipient.toLowerCase() === options.to.toLowerCase(),
        ) &&
        (options.subjectContains === undefined ||
          message.subject
            .toLowerCase()
            .includes(options.subjectContains.toLowerCase())),
    );

    if (match) {
      return match;
    }

    await new Promise(resolveDelay => setTimeout(resolveDelay, pollIntervalMs));
  }

  const received = (await readMessages())
    .map(
      message =>
        `"${message.subject}" to ${message.to.join(', ') || '(nobody)'}`,
    )
    .join('; ');

  throw new Error(
    `No message to ${options.to}${options.subjectContains ? ` with a subject containing "${options.subjectContains}"` : ''} arrived within ${timeoutMs}ms. Received: ${received || 'nothing'}.`,
  );
}

/**
 * Removes every message the sink holds, for a test that must not read a message an earlier test caused.
 */
export async function clearMessages(): Promise<void> {
  await fetch(`${mailSinkUrl}/email/all`, { method: 'DELETE' });
}

/**
 * Reads the one-time code from a message that carries one.
 *
 * @param message The message to read.
 * @returns The six-digit code the application will accept.
 */
export function oneTimeCode(message: SinkMessage): string {
  const code = secretBlocks(message).find(block => /^\d{6}$/.test(block));

  if (!code) {
    throw new Error(
      `No six-digit code was found in the message "${message.subject}". Blocks found: ${secretBlocks(message).join(', ') || 'none'}.`,
    );
  }

  return code;
}

/**
 * Reads the temporary password from a message that carries one - a sign-up's generated password, or an invitation's.
 *
 * @param message The message to read.
 * @returns The password to sign in with.
 */
export function temporaryPassword(message: SinkMessage): string {
  const password = secretBlocks(message)
    .filter(block => !/^\d{6}$/.test(block))
    .pop();

  if (!password) {
    throw new Error(
      `No temporary password was found in the message "${message.subject}". Blocks found: ${secretBlocks(message).join(', ') || 'none'}.`,
    );
  }

  return password;
}

function secretBlocks(message: SinkMessage): string[] {
  const blocks: string[] = [];

  for (const match of message.html.matchAll(monospacedBlockPattern)) {
    blocks.push(match[1].trim());
  }

  return blocks;
}
