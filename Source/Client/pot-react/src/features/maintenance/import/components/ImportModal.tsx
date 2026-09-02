import { Upload } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { ErrorToast, SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { WindowOpenFile } from '@/lib';

import { useImport } from '../hooks/useImport';

type ImportModalProps = {
  isOpen: boolean;
  onClose: () => void;
};

function ImportModal({ isOpen, onClose }: ImportModalProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isImporting, setIsImporting] = useState(false);
  const { setError } = useErrorContext();
  const { importData, isPending } = useImport();

  async function handleFileSelect() {
    // Check if File System Access API is supported
    if ('showOpenFilePicker' in window) {
      try {
        const [fileHandle] = await (
          window as WindowOpenFile
        ).showOpenFilePicker({
          types: [
            {
              description: 'POT Export files',
              accept: {
                'application/vnd.pot.export': ['.export'],
              },
            },
          ],
          multiple: false,
        });

        const file = await fileHandle.getFile();
        setSelectedFile(file);
      } catch (error) {
        if (error instanceof Error && error.name === 'AbortError') {
          // User cancelled, do nothing
          return;
        }

        logger.error('ImportModal', 'Error selecting file', error);
      }
    } else {
      // Fallback to traditional file input for browsers that don't support File System Access API
      const input = document.createElement('input');
      input.type = 'file';
      input.accept = '.export';
      input.onchange = e => {
        const file = (e.target as HTMLInputElement).files?.[0];

        if (file) {
          setSelectedFile(file);
        }
      };

      input.click();
    }
  }

  async function handleImport() {
    if (!selectedFile) {
      return;
    }

    setIsImporting(true);

    // This will invalidate the query cache
    const result = await importData(selectedFile);

    if (result.success) {
      toast(
        <SuccessToast
          icon={Upload}
          title="Import Complete"
          description="Data imported successfully"
          details={`${result.value.imported} records imported`}
        />,
        { duration: 5000 },
      );
    } else {
      toast(
        <ErrorToast
          title="Import Failed"
          description="There was an error importing the data."
        />,
        { duration: 5000 },
      );

      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }

    setIsImporting(false);
    handleClose();
  }

  function handleClose() {
    // Call onClose() first to trigger dialog unmount. If setSelectedFile is called first, the state
    // update may be ignored or cause a warning if the component unmounts immediately. This order
    // ensures the dialog closes properly and the selected file state is reset for the next open.
    onClose();
    setSelectedFile(null);
  }

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Upload className="h-5 w-5" />
            Import Financial Data
          </DialogTitle>
          <DialogDescription>
            Select a previously exported zip file to import your financial data.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 mt-4">
          <div>
            <Label className="block gap-0 mb-2">Select Export File</Label>
            <div className="flex gap-2 mt-2">
              <Input
                value={selectedFile?.name ?? ''}
                readOnly
                placeholder="Click Browse to select a .export file"
                className="flex-1"
              />
              <Button variant="outline" onClick={handleFileSelect}>
                Browse
              </Button>
            </div>
            <p className="text-xs text-muted-foreground mt-1 h-4">
              {selectedFile
                ? `File size: ${Math.round(selectedFile.size / 1024)} KB`
                : ''}
            </p>
          </div>
        </div>

        <DialogFooter className="gap-4">
          <Button variant="outline" onClick={handleClose} className="w-28">
            Cancel
          </Button>
          <Button
            onClick={handleImport}
            disabled={!selectedFile || isPending || isImporting}
            className="w-28"
          >
            Import Data
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export { ImportModal };
