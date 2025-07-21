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
      toast.success('Export Successful', {
        description: `Data exported to ${result.value.filename} successfully.`,
      });
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
          <DialogDescription>
            Export all your financial data as a zip file that can be saved
            locally or imported later.
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <p className="text-sm text-muted-foreground">
            This will create a complete backup of your financial data including
            expenses, income, and account information.
          </p>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={handleExport} disabled={isLoading}>
            {isLoading ? 'Exporting...' : 'Export Data'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export { ExportModal };
