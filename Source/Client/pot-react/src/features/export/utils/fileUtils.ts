import { WindowSaveFile } from '@/lib';

class FileOperationCancelledError extends Error {
  constructor(message = 'File operation was cancelled by the user') {
    super(message);
    this.name = 'FileOperationCancelledError';
  }
}

/**
 * Downloads a blob as a file with the specified filename
 * Returns a Promise that resolves when file is saved or rejects when cancelled
 * @throws {FileOperationCancelledError} When the user cancels the file save operation
 */
async function downloadBlob(blob: Blob, filename: string): Promise<void> {
  // Check if File System Access API is supported
  if ('showSaveFilePicker' in window) {
    try {
      const fileHandle = await (window as WindowSaveFile).showSaveFilePicker({
        suggestedName: filename,
        types: [
          {
            description: 'POT Export files',
            accept: {
              'application/vnd.pot.export': ['.export'],
            },
          },
        ],
      });

      const writable = await fileHandle.createWritable();
      await writable.write(blob);
      await writable.close();
      return; // File saved successfully
    } catch (error) {
      // User cancelled or error occurred
      if (error instanceof Error && error.name === 'AbortError') {
        throw new FileOperationCancelledError();
      }
      throw error;
    }
  } else {
    // Fallback to traditional download for browsers that don't support File System Access API
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);

    // For fallback, we can't detect cancellation, so we resolve immediately
    return;
  }
}

function extractFilename(headers: Record<string, string>): string {
  const contentDisposition = headers['content-disposition'];

  if (contentDisposition) {
    // Matches quoted filenames and captures everything between quotes
    // eg: filename="pot-export-2025-07-16-10-21-46.zip"
    let filenameMatch = contentDisposition.match(/filename="([^"]+)"/);

    if (filenameMatch) {
      return filenameMatch[1];
    }

    // Matches unquoted filenames and captures everything until the first semicolon or end of string
    // eg: filename=pot-export-2025-07-16-10-21-46.zip
    filenameMatch = contentDisposition.match(/filename=([^;]+)/);

    if (filenameMatch) {
      return filenameMatch[1].trim();
    }
  }

  return 'financial-data-export.zip';
}

export { downloadBlob, extractFilename, FileOperationCancelledError };
