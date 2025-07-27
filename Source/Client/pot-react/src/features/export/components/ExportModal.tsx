import { Download } from 'lucide-react';
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

import { useExport } from '../hooks/useExport';

type ExportModalProps = {
  isOpen: boolean;
  onClose: () => void;
};

function ExportModal({ isOpen, onClose }: ExportModalProps) {
  const { exportData, isLoading } = useExport();

  async function handleExport() {
    const result = await exportData();

    if (result.success) {
      toast(
        <SuccessToast
          icon={Download}
          title="Export Complete"
          description="Data exported successfully"
          details={result.value.filename}
        />,
        { duration: 5000 },
      );
    } else {
      toast(
        <ErrorToast
          icon={Download}
          title="Export Failed"
          description="There was an error exporting the data."
        />,
        { duration: 5000 },
      );
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
