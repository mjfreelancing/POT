import { expect, test } from '../../fixtures/auth';

// Covers the export flow: trigger Export... from the
// sidebar and assert a download fires with the server-provided file name.
// Export is a read-only GET (no shared state mutation) -> parallel-safe, so
// this is a plain test with no fixture teardown.
//
// The failure path (GET /api/maintenance/export -> 500 -> "Export Failed"
// toast) is already covered by feedback/toasts.test.ts, so this focuses
// on the successful download + file-name contract.
//
// Desktop-only: the Export... action is hidden on mobile.

test('export downloads the financial data with the server file name', async ({
  page,
}, testInfo) => {
  test.skip(
    testInfo.project.name.startsWith('mobile'),
    'Export is desktop-only; mobile uses the card grid',
  );

  // The export hook uses the File System Access API (showSaveFilePicker) when
  // available, which opens a NATIVE save dialog Playwright cannot capture or
  // dismiss. Remove it so the fallback <a download> path runs and a Playwright
  // 'download' event fires. Covers both an own property and a prototype method.
  await page.addInitScript(() => {
    const win = window as unknown as { showSaveFilePicker?: unknown };
    delete win.showSaveFilePicker;

    const proto = Object.getPrototypeOf(window) as unknown as {
      showSaveFilePicker?: unknown;
    };
    delete proto.showSaveFilePicker;
  });

  await page.goto('/dashboard');

  // Sidebar Maintenance group -> Export... opens the export dialog.
  await page.getByRole('button', { name: 'Export...' }).click();

  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible();
  await expect(
    dialog.getByText('Export Financial Data', { exact: true }),
  ).toBeVisible();

  // Register the download listener BEFORE clicking Export Data.
  const downloadPromise = page.waitForEvent('download');

  await dialog.getByRole('button', { name: 'Export Data' }).click();

  const download = await downloadPromise;

  // Server names the file pot-<yyyy-MM-dd_HHmmss>.export (Handler.cs).
  expect(download.suggestedFilename()).toMatch(
    /^pot-\d{4}-\d{2}-\d{2}_\d{6}\.export$/,
  );

  // Success toast confirms the export completed.
  await expect(
    page.getByText('Export Complete', { exact: true }),
  ).toBeVisible();
});
