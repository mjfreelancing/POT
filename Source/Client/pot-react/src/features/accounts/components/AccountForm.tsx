import { UseFormReturn } from 'react-hook-form';

import MoneyValueInput, {
  MoneyValueChangeEvent,
} from '@/components/input/MoneyValueInput';
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

import { AccountFormData } from '../schemas/accountFormSchema';

type AccountFormProps = {
  form: UseFormReturn<AccountFormData>;
  onSubmit: (values: AccountFormData) => Promise<void>;
  onCancel: () => void;
  readOnlyAccountIdentifiers: boolean;
  submitLabel: string;
};

const AccountForm = ({
  form,
  onSubmit,
  onCancel,
  readOnlyAccountIdentifiers,
  submitLabel,
}: AccountFormProps) => (
  <Form {...form}>
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
      <FormField
        control={form.control}
        name="bsb"
        render={({ field }) => (
          <FormItem className="space-y-1">
            <FormLabel htmlFor="bsb-input">BSB</FormLabel>
            <FormControl>
              <Input
                {...field}
                id="bsb-input"
                placeholder={
                  readOnlyAccountIdentifiers
                    ? undefined
                    : 'Enter the Account BSB'
                }
                aria-description="Enter BSB in the format XXX-XXX"
                readOnly={readOnlyAccountIdentifiers}
                className={
                  readOnlyAccountIdentifiers
                    ? 'bg-muted cursor-not-allowed'
                    : undefined
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
            <FormLabel htmlFor="account-number-input">Account Number</FormLabel>
            <FormControl>
              <Input
                {...field}
                id="account-number-input"
                placeholder={
                  readOnlyAccountIdentifiers
                    ? undefined
                    : 'Enter the Account Number'
                }
                aria-description="Your bank account number"
                readOnly={readOnlyAccountIdentifiers}
                className={
                  readOnlyAccountIdentifiers
                    ? 'bg-muted cursor-not-allowed'
                    : undefined
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

      <div className="flex justify-end space-x-4">
        {/* type="button" prevents this button from triggering a form submission - there's a scenario
            where the user may press ENTER but the server reports a validation error and the sheet
            closes, thereby not providing the user an opportunity to correct the data. */}
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          className="w-24"
        >
          Cancel
        </Button>

        <Button type="submit" className="w-24">
          {submitLabel}
        </Button>
      </div>
    </form>
  </Form>
);

export { AccountForm };
