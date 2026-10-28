import { spawnSync } from 'node:child_process';
import { connect } from 'node:net';

/**
 * The container that receives the messages a run generates.
 *
 * The API is already configured to deliver here by `playwright.config.ts` (`Smtp__Host=127.0.0.1`,
 * `Smtp__Port=1025`), so the sink's shape is fixed: it receives on 1025 and is read back over HTTP on 1080. The
 * container is named rather than matched, so the development Docker stack is never a candidate for replacing or
 * removing.
 */
export const mailSinkContainerName = 'pot-e2e-mail-sink';

const mailSinkImage = 'maildev/maildev:2.2.1';
const mailSinkSmtpPort = 1025;
const mailSinkReadPort = 1080;

// Must match the API's Smtp__Authentication__* values in playwright.config.ts.
const mailSinkSmtpUsername = 'e2e';
const mailSinkSmtpPassword = 'e2e';

/** Where a test reads the messages back from. */
export const mailSinkUrl = `http://127.0.0.1:${mailSinkReadPort}`;

const mailSinkReadinessTimeoutMs = 30_000;
const mailSinkReadinessIntervalMs = 250;

function docker(args: string[]): { ok: boolean; output: string } {
  const result = spawnSync('docker', args, { encoding: 'utf-8' });

  return {
    ok: result.status === 0,
    output: `${result.stdout ?? ''}${result.stderr ?? ''}`.trim(),
  };
}

function isRunning(containerName: string): boolean {
  const result = docker([
    'ps',
    '--filter',
    `name=${containerName}`,
    '--format',
    '{{.Names}}',
  ]);

  return result.ok && result.output.split(/\r?\n/).includes(containerName);
}

/**
 * Strictly the process that holds a port, so a run that cannot have the port says who has it instead of failing
 * later with a connection error.
 */
function describePortOwner(port: number): string {
  const script = [
    `$connection = Get-NetTCPConnection -State Listen -LocalPort ${port} -ErrorAction SilentlyContinue | Select-Object -First 1`,
    'if (-not $connection) { exit 3 }',
    '$process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue',
    `Write-Output ("pid {0} {1}" -f $connection.OwningProcess, $process.ProcessName)`,
  ].join('; ');

  const result = spawnSync(
    'pwsh',
    ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', script],
    { encoding: 'utf-8' },
  );

  const owner = (result.stdout ?? '').trim();

  return owner.length > 0 ? owner : 'an unidentified process';
}

function isPortAcceptingConnections(port: number): Promise<boolean> {
  return new Promise(resolvePortCheck => {
    const socket = connect({ host: '127.0.0.1', port });

    socket.setTimeout(1000);

    socket.once('connect', () => {
      socket.destroy();
      resolvePortCheck(true);
    });

    socket.once('timeout', () => {
      socket.destroy();
      resolvePortCheck(false);
    });

    socket.once('error', () => {
      socket.destroy();
      resolvePortCheck(false);
    });
  });
}

async function waitForReadiness(): Promise<void> {
  const deadline = Date.now() + mailSinkReadinessTimeoutMs;

  while (Date.now() < deadline) {
    try {
      const response = await fetch(`${mailSinkUrl}/email`);

      if (response.ok) {
        return;
      }
    } catch {
      // Not listening yet.
    }

    await new Promise(resolveDelay =>
      setTimeout(resolveDelay, mailSinkReadinessIntervalMs),
    );
  }

  throw new Error(
    `The mail sink did not answer at ${mailSinkUrl} within ${mailSinkReadinessTimeoutMs}ms.`,
  );
}

/**
 * Starts the run's mail sink, replacing one an interrupted run left behind.
 *
 * A leftover is reported rather than silently tolerated: it is the harness's own container, so replacing it is
 * safe and is what makes the run start with an empty inbox. Anything else holding either port is a failure before
 * any test runs, and it names the process.
 */
export async function startMailSink(): Promise<void> {
  if (isRunning(mailSinkContainerName)) {
    console.log(
      `🧹 Mail sink left behind by a previous run detected: ${mailSinkContainerName} - replacing it`,
    );

    docker(['rm', '-f', mailSinkContainerName]);
  }

  for (const port of [mailSinkSmtpPort, mailSinkReadPort]) {
    if (await isPortAcceptingConnections(port)) {
      throw new Error(
        `The mail sink cannot bind port ${port}: it is held by ${describePortOwner(port)}. Stop it, or run 'npm run e2e:clean' if it is this harness's own sink.`,
      );
    }
  }

  const started = docker([
    'run',
    '-d',
    '--name',
    mailSinkContainerName,
    '-p',
    `127.0.0.1:${mailSinkSmtpPort}:${mailSinkSmtpPort}`,
    '-p',
    `127.0.0.1:${mailSinkReadPort}:${mailSinkReadPort}`,
    mailSinkImage,
    // The API is started with SMTP credentials (startup validation requires the section to be bound), so the sink
    // must accept them: without AUTH it answers "The SMTP server does not support authentication" and every message
    // is lost before it is stored.
    '--incoming-user',
    mailSinkSmtpUsername,
    '--incoming-pass',
    mailSinkSmtpPassword,
  ]);

  if (!started.ok) {
    throw new Error(`The mail sink could not be started: ${started.output}`);
  }

  await waitForReadiness();

  console.log(
    `📬 Mail sink ready - receiving on ${mailSinkSmtpPort}, read back from ${mailSinkUrl}`,
  );
}

/**
 * Removes the run's mail sink. The next run starts an empty one, so a message can never be read twice by accident.
 */
export function stopMailSink(): void {
  if (!isRunning(mailSinkContainerName)) {
    return;
  }

  const removed = docker(['rm', '-f', mailSinkContainerName]);

  if (!removed.ok) {
    console.warn(`⚠️  Could not remove the mail sink: ${removed.output}`);
    return;
  }

  console.log('🧹 Mail sink removed');
}
