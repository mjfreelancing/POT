import { Download } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

import { useExport } from '../hooks/useExport';

type ExportModalProps = {
  isOpen: boolean;
  onClose: () => void;
};

function ExportModal({ isOpen, onClose }: ExportModalProps) {
  const { exportData, isLoading } = useExport();

  async function handleExport() {
    const result = await exportData();

    // TODO: Make the toast messages look better
    if (result.success) {
      toast(
        <div className="flex items-start">
          <Download className="text-green-600 mr-6 w-16 h-16" />
          <div>
            <div className="text-xl font-semibold">Export Complete</div>
            <div className="mt-2 text-sm text-muted-foreground">
              Data exported successfully
            </div>
            <div className="mt-1 text-xs font-mono text-muted-foreground">
              {result.value.filename}
            </div>
          </div>
        </div>,
        { duration: 5000 },
      );
    } else {
      toast.error('Export Failed', {
        description: 'There was an error exporting the data. Please try again.',
      });
    }

    onClose();
  }

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Download className="h-5 w-5" />
            Export Financial Data
          </DialogTitle>
          <DialogDescription className="mb-2">
            Export all your financial data to prevent accidental data loss.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="gap-4">
          <Button variant="outline" onClick={onClose} className="w-28">
            Cancel
          </Button>
          <Button onClick={handleExport} disabled={isLoading} className="w-28">
            {isLoading ? 'Exporting...' : 'Export Data'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export { ExportModal };
