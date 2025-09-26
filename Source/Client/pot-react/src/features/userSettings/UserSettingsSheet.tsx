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

import { AccountUserInfo, ChangePasswordForm } from './sections';

type AccountSettingsSheetProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onClose: () => void;
};

function AccountSettingsSheet(props: AccountSettingsSheetProps): JSX.Element {
  const { open, onOpenChange, onClose } = props;

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
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
          <AccountUserInfo />
          <div className="border border-border rounded-lg bg-background">
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
        </div>
      </SheetContent>
    </Sheet>
  );
}

export { AccountSettingsSheet };
