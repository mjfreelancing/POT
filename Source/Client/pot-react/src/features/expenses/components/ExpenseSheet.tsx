import { DialogDescription, DialogTitle } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Sheet, SheetContent } from '@/components/ui/sheet';
import { FORM_SHEET_STYLES } from '@/lib';

type ExpenseSheetProps = {
  title: string;
  children: React.ReactNode;
};

function ExpenseSheet({ title, children }: ExpenseSheetProps) {
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
      <SheetContent className={FORM_SHEET_STYLES.SHEET_CONTENT}>
        <div className={FORM_SHEET_STYLES.SHEET_INNER}>
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

export default ExpenseSheet;
