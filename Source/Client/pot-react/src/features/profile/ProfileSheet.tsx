import { JSX } from 'react';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetClose,
} from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { XIcon } from 'lucide-react';
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { ChangePasswordForm } from './changePassword/components/ChangePasswordForm';

type ProfileSheetProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onClose: () => void;
};

function ProfileSheet(props: ProfileSheetProps): JSX.Element {
  const { open, onOpenChange, onClose } = props;

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      {/* Hide the default close (X) button using [&>button:first-of-type]:hidden */}
      <SheetContent
        side="right"
        className="w-full max-w-md p-6 h-full flex flex-col [&>button:first-of-type]:hidden"
      >
        <div className="flex items-center justify-between mb-6">
          <SheetTitle className="text-2xl font-bold tracking-tight text-primary">
            Profile
          </SheetTitle>
          <SheetClose asChild>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Close profile"
              onClick={onClose}
            >
              <XIcon className="size-5" />
            </Button>
          </SheetClose>
        </div>
        {/* This style causes the accordion to fill the vertical space and scroll when required */}
        <div className="flex-1 min-h-0 overflow-y-auto">
          <Accordion type="single" collapsible defaultValue="change-password">
            <AccordionItem className="px-4" value="change-password">
              <AccordionTrigger className="text-lg font-semibold text-primary">
                Change Password
              </AccordionTrigger>
              <AccordionContent className="px-4 pt-4">
                <ChangePasswordForm />
              </AccordionContent>
            </AccordionItem>
            {/* Add more AccordionItem components here as needed */}
          </Accordion>
        </div>
        {/* Close button moved to header */}
      </SheetContent>
    </Sheet>
  );
}

export { ProfileSheet };
