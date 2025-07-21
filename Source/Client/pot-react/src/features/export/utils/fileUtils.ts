/**
 * Downloads a blob as a file with the specified filename
 */
function downloadBlob(blob: Blob, filename: string): void {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
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

export { downloadBlob, extractFilename };
