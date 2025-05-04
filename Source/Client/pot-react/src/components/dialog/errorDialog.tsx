import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { noop } from '@/lib/utils';

// Useful for setting state to be provided to an ErrorDialog, such as:
// const [dialogEror, setDialogError] = useState<ErrorDialogState | null>(null);
export type ErrorDialogState = {
  title: string;
  description: string;
};

type ErrorDialogProps = {
  open: boolean;
  title: string;
  description: string;
  onOk?: () => void;
};

function ErrorDialog({
  open,
  title = 'Error',
  description,
  onOk = noop,
}: ErrorDialogProps) {
  return (
    <AlertDialog open={open}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogAction onClick={onOk}>OK</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

export { ErrorDialog };
