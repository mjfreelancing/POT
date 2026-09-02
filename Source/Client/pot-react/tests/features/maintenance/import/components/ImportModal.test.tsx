import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { toast } from 'sonner';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { useErrorContext } from '@/contexts';
import { ImportModal } from '@/features/maintenance/import/components/ImportModal';
import { useImport } from '@/features/maintenance/import/hooks/useImport';

vi.mock('sonner', () => ({
  toast: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('@/contexts', () => ({
  useErrorContext: vi.fn(),
}));

vi.mock('@/features/maintenance/import/hooks/useImport', () => ({
  useImport: vi.fn(),
}));

const onCloseMock = vi.fn();
const importDataMock = vi.fn();
const setErrorMock = vi.fn();

function createExportFile() {
  return new File(['pot-export-data'], 'backup.export', {
    type: 'application/vnd.pot.export',
  });
}

async function selectFileViaPicker(file: File) {
  const showOpenFilePickerMock = vi.fn().mockResolvedValue([
    {
      getFile: vi.fn().mockResolvedValue(file),
    },
  ]);

  vi.stubGlobal('showOpenFilePicker', showOpenFilePickerMock);

  await userEvent.click(screen.getByRole('button', { name: 'Browse' }));
}

describe('ImportModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useImport).mockReturnValue({
      importData: importDataMock,
      isPending: false,
    } as unknown as ReturnType<typeof useImport>);

    vi.mocked(useErrorContext).mockReturnValue({
      error: null,
      setError: setErrorMock,
    });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  test('shows the browse hint while no file is selected and keeps import disabled', () => {
    render(<ImportModal isOpen onClose={onCloseMock} />);

    const fileInput = screen.getByPlaceholderText(
      'Click Browse to select a .export file',
    );

    expect(fileInput).toHaveValue('');
    expect(
      screen.getByRole('button', { name: 'Import Data' }),
    ).toBeDisabled();
  });

  test('shows the selected file name and enables import once a file is chosen', async () => {
    render(<ImportModal isOpen onClose={onCloseMock} />);

    const file = createExportFile();

    await selectFileViaPicker(file);

    const fileInput = screen.getByPlaceholderText(
      'Click Browse to select a .export file',
    );

    await waitFor(() => expect(fileInput).toHaveValue('backup.export'));

    expect(screen.getByRole('button', { name: 'Import Data' })).toBeEnabled();
  });

  test('imports the selected file and closes the dialog on success', async () => {
    importDataMock.mockResolvedValue({
      success: true,
      value: { imported: 7 },
    });

    render(<ImportModal isOpen onClose={onCloseMock} />);

    const file = createExportFile();

    await selectFileViaPicker(file);
    await userEvent.click(screen.getByRole('button', { name: 'Import Data' }));

    await waitFor(() => expect(onCloseMock).toHaveBeenCalled());

    expect(importDataMock).toHaveBeenCalledWith(file);
    expect(vi.mocked(toast)).toHaveBeenCalled();
  });

  test('surfaces an import failure through the shared error context', async () => {
    importDataMock.mockResolvedValue({
      success: false,
      error: {
        code: 'IMPORT_FAILED',
        description: 'The file could not be imported.',
      },
    });

    render(<ImportModal isOpen onClose={onCloseMock} />);

    const file = createExportFile();

    await selectFileViaPicker(file);
    await userEvent.click(screen.getByRole('button', { name: 'Import Data' }));

    await waitFor(() =>
      expect(setErrorMock).toHaveBeenCalledWith({
        title: 'IMPORT_FAILED',
        description: 'The file could not be imported.',
      }),
    );

    expect(onCloseMock).toHaveBeenCalled();
  });
});
