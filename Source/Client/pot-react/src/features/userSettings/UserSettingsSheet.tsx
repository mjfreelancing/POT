import { XIcon } from 'lucide-react';
import { JSX } from 'react';

import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Button } from '@/components/ui/button';
import { DialogDescription, DialogTitle } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Sheet, SheetClose, SheetContent } from '@/components/ui/sheet';

import ChangePasswordForm from './sections/changePassword/ChangePasswordForm';
import UserDetailsForm from './sections/userDetails/UserDetailsForm';

type AccountSettingsSheetProps = {
  open: boolean;
  onClose: () => void;
};

function AccountSettingsSheet(props: AccountSettingsSheetProps): JSX.Element {
  const { open, onClose } = props;

  return (
    <Sheet
      open={open}
      onOpenChange={() => {
        /* Prevent closing on outside click */
      }}
    >
      {/* Hide the default close (X) button using [&>button:first-of-type]:hidden */}
      <SheetContent
        side="right"
        className="p-6 sm:max-w-lg [&>button:first-of-type]:hidden"
      >
        <div className="space-y-6 pr-6 pl-6">
          <div className="flex items-center justify-between">
            <DialogTitle className="text-lg font-semibold">
              Account Settings
            </DialogTitle>
            <DialogDescription className="sr-only">
              Manage your account settings
            </DialogDescription>
            <SheetClose asChild>
              <Button
                variant="ghost"
                size="icon"
                aria-label="Close account settings"
                onClick={onClose}
              >
                <XIcon className="size-5" />
              </Button>
            </SheetClose>
          </div>

          <Separator />

          <Accordion type="single" collapsible defaultValue="user-details">
            <div className="border border-border rounded-lg bg-background">
              <AccordionItem className="px-4" value="user-details">
                <AccordionTrigger className="text-lg font-semibold text-primary">
                  User Details
                </AccordionTrigger>
                <AccordionContent className="flex flex-col gap-4">
                  <UserDetailsForm />
                </AccordionContent>
              </AccordionItem>
            </div>

            <div className="py-4" />

            <div className="border border-border rounded-lg bg-background">
              <AccordionItem className="px-4" value="change-password">
                <AccordionTrigger className="text-lg font-semibold text-primary">
                  Change Password
                </AccordionTrigger>
                <AccordionContent className="flex flex-col gap-4">
                  <ChangePasswordForm />
                </AccordionContent>
              </AccordionItem>
            </div>
          </Accordion>
        </div>
      </SheetContent>
    </Sheet>
  );
}

export { AccountSettingsSheet };
