import { JSX } from 'react';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetClose,
} from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';

type ProfileSheetProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onClose: () => void;
};

function ProfileSheet(props: ProfileSheetProps): JSX.Element {
  const { open, onOpenChange, onClose } = props;

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="w-full max-w-md">
        <SheetHeader>
          <SheetTitle>Profile</SheetTitle>
        </SheetHeader>
        <div className="py-4">
          <p className="text-muted-foreground mb-4">
            Profile management coming soon.
          </p>
          <SheetClose asChild>
            <Button variant="outline" onClick={onClose} className="mt-2">
              Close
            </Button>
          </SheetClose>
        </div>
      </SheetContent>
    </Sheet>
  );
}

export { ProfileSheet };
