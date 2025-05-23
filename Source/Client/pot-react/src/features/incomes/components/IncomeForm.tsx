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

import { IncomeFormSchema } from '../schemas/incomeSchema';

type IncomeFormProps = {
  form: UseFormReturn<IncomeFormSchema>;
  onSubmit: (values: IncomeFormSchema) => Promise<void>;
  onCancel: () => void;
  readOnlyIncomeIdentifiers: boolean;
  submitLabel: string;
};

const IncomeForm = ({
  form,
  onSubmit,
  onCancel,
  readOnlyIncomeIdentifiers,
  submitLabel,
}: IncomeFormProps) => (
  <Form {...form}>
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
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
                placeholder={
                  readOnlyIncomeIdentifiers ? undefined : 'Enter a description'
                }
                aria-description="A memorable name for this income"
                readOnly={readOnlyIncomeIdentifiers}
                className={
                  readOnlyIncomeIdentifiers
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
        name="amount"
        render={({ field }) => (
          <FormItem className="space-y-1">
            <FormLabel htmlFor="amount-input">Amount</FormLabel>
            <FormControl>
              {/* Getting the numerical value from e.target.number since the 'value' is a string */}
              <MoneyValueInput
                {...field}
                id="amount-input"
                placeholder="Enter the income amount"
                aria-description="Current income amount"
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

export { IncomeForm };
