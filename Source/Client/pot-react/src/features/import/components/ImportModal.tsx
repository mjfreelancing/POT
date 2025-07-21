import { Upload } from 'lucide-react';
import { useState } from 'react';

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

import { useImport } from '../hooks/useImport';
import { toast } from 'sonner';

type ImportModalProps = {
  isOpen: boolean;
  onClose: () => void;
};

function ImportModal({ isOpen, onClose }: ImportModalProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const { importData, isLoading } = useImport();

  function handleFileSelect() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.zip';
    input.onchange = e => {
      const file = (e.target as HTMLInputElement).files?.[0];

      if (file) {
        setSelectedFile(file);
      }
    };

    input.click();
  }

  async function handleImport() {
    if (!selectedFile) {
      return;
    }

    const result = await importData(selectedFile);

    // TODO: Make the toast messages look better
    if (result.success) {
      toast.success('Import Successful', {
        description: `Imported ${result.value.imported} records.`,
      });
    } else {
      toast.error('Import Failed', {
        description:
          'There was an error importing the data. Please check the file and try again.',
      });
    }

    handleClose();
  }

  function handleClose() {
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

        <div className="space-y-4">
          <div>
            <Label>Select Export File</Label>
            <div className="flex gap-2 mt-2">
              <Input
                value={selectedFile?.name || 'No file selected'}
                readOnly
                placeholder="Click Browse to select a zip file"
                className="flex-1"
              />
              <Button variant="outline" onClick={handleFileSelect}>
                Browse
              </Button>
            </div>
            {selectedFile && (
              <p className="text-xs text-muted-foreground mt-1">
                File size: {Math.round(selectedFile.size / 1024)} KB
              </p>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            Cancel
          </Button>
          <Button onClick={handleImport} disabled={!selectedFile || isLoading}>
            {isLoading ? 'Importing...' : 'Import Data'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export { ImportModal };
