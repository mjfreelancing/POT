import { useEffect } from 'react';
import type { UseFormReturn } from 'react-hook-form';

import type { MoneyValueChangeEvent } from '@/components/input';
import { BsbInput, MoneyValueInput } from '@/components/input';
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import { FORM_SHEET_STYLES } from '@/lib';

import type { AccountFormData } from '../schemas/accountFormSchema';

type AccountFormProps = {
  form: UseFormReturn<AccountFormData>;
  onSubmit: (values: AccountFormData) => Promise<void>;
  onCancel: () => void;
  isEditMode: boolean;
  submitLabel: string;
};

function AccountForm({
  form,
  onSubmit,
  onCancel,
  submitLabel,
  isEditMode = false,
}: AccountFormProps) {
  useEffect(() => {
    if (isEditMode) {
      form.setFocus('balance');
    }
  }, [isEditMode, form]);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="bsb"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="bsb-input">BSB</FormLabel>
              <FormControl>
                {/* Masked input normalizes BSB to XXX-XXX whether the user types
                    digits with or without the separating dash. */}
                <BsbInput
                  {...field}
                  id="bsb-input"
                  placeholder={isEditMode ? undefined : 'XXX-XXX'}
                  aria-description="Enter BSB in the format XXX-XXX"
                  readOnly={isEditMode}
                  className={
                    isEditMode ? 'bg-muted cursor-not-allowed' : undefined
                  }
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="number"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="account-number-input">
                Account Number
              </FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="account-number-input"
                  placeholder={
                    isEditMode ? undefined : 'Enter the Account Number'
                  }
                  aria-description="Your bank account number"
                  readOnly={isEditMode}
                  className={
                    isEditMode ? 'bg-muted cursor-not-allowed' : undefined
                  }
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="description-input">Description</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="description-input"
                  placeholder="Enter a description"
                  aria-description="A memorable name for this account"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="balance"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="balance-input">Balance</FormLabel>
              <FormControl>
                {/* Getting the numerical value from e.target.number since the 'value' is a string */}
                <MoneyValueInput
                  {...field}
                  id="balance-input"
                  placeholder="Enter the Account balance"
                  aria-description="Current account balance"
                  onChange={(e: MoneyValueChangeEvent) => {
                    field.onChange(e.target.number);
                  }}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="reserved"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="reserved-input">Reserved</FormLabel>
              <FormControl>
                {/* Getting the numerical value from e.target.number since the 'value' is a string */}
                <MoneyValueInput
                  {...field}
                  id="reserved-input"
                  placeholder="Enter the reserved amount"
                  aria-description="Amount reserved for unexpected expenses"
                  onChange={(e: MoneyValueChangeEvent) => {
                    field.onChange(e.target.number);
                  }}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className={FORM_SHEET_STYLES.ACTION_SECTION}>
          <Separator className="opacity-80" />
          <div className={FORM_SHEET_STYLES.ACTION_ROW}>
            {/* type="button" prevents this button from triggering a form submission - there's a scenario
              where the user may press ENTER but the server reports a validation error and the sheet
              closes, thereby not providing the user an opportunity to correct the data. */}
            <Button
              type="button"
              variant="outline"
              onClick={onCancel}
              className={FORM_SHEET_STYLES.ACTION_BUTTON_WIDTH}
            >
              Cancel
            </Button>

            <Button
              type="submit"
              className={FORM_SHEET_STYLES.ACTION_BUTTON_WIDTH}
            >
              {submitLabel}
            </Button>
          </div>
        </div>
      </form>
    </Form>
  );
}

export default AccountForm;
