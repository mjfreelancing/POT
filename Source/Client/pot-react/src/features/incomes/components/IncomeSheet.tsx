import { DialogDescription, DialogTitle } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Sheet, SheetContent } from '@/components/ui/sheet';

type IncomeSheetProps = {
  title: string;
  children: React.ReactNode;
};

function IncomeSheet({ title, children }: IncomeSheetProps) {
  return (
    <Sheet open modal={false}>
      {/*
      You can adjust the width by choosing a different size:
      - sm:max-w-md (28rem)
      - sm:max-w-lg (32rem)
      - sm:max-w-xl (36rem)
      - sm:max-w-2xl (42rem)
      - sm:max-w-3xl (48rem)

      The default width is 24rem (sm:max-w-sm).

      The close button (the first button) is hidden using [&>button:first-of-type]:hidden
    */}
      <SheetContent className="p-6 sm:max-w-lg [&>button:first-of-type]:hidden">
        <div className="space-y-6 pr-6 pl-6">
          <DialogTitle className="text-lg font-semibold">{title}</DialogTitle>
          <DialogDescription className="sr-only">
            {title} form
          </DialogDescription>
          <Separator />
          {children}
        </div>
      </SheetContent>
    </Sheet>
  );
}

export default IncomeSheet;
