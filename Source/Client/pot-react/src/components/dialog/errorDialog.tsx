import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';

// A no-op named function is required to avoid the linter error 'Unexpected empty arrow function'
const noop = () => void 0;

type ErrorDialogProps = {
  open: boolean;
  title: string;
  description: string;
  onOk?: () => void;
};

export function ErrorDialog({
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
