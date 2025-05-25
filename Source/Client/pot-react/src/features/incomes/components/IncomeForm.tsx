import { UseFormReturn } from 'react-hook-form';

import MoneyValueInput, {
  MoneyValueChangeEvent,
} from '@/components/input/MoneyValueInput';
import { EnrichedDatePicker } from '@/components/picker/EnrichedDatePicker';
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { Account } from '@/data/accounts/account';
import { FrequencyEnumValues } from '@/lib/types';
import { localIsoDate, localToday } from '@/lib/utils';

import { IncomeFormSchema } from '../schemas/incomeSchema';

type IncomeFormProps = {
  form: UseFormReturn<IncomeFormSchema>;
  onSubmit: (values: IncomeFormSchema) => Promise<void>;
  onCancel: () => void;
  readOnlyIncomeIdentifiers: boolean;
  submitLabel: string;
  accounts: Account[];
};

const IncomeForm = ({
  form,
  onSubmit,
  onCancel,
  readOnlyIncomeIdentifiers,
  submitLabel,
  accounts,
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

      <FormField
        control={form.control}
        name="nextDue"
        render={({ field }) => (
          <FormItem className="space-y-1">
            <FormLabel htmlFor="nextDue-picker">Next Due</FormLabel>
            <FormControl>
              <div className="flex items-center space-x-2">
                <EnrichedDatePicker
                  selectedDate={field.value ? new Date(field.value) : undefined}
                  onDateAccepted={field.onChange}
                  triggerClassName="flex-1"
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="w-16"
                  onClick={() => field.onChange(localToday())}
                >
                  Today
                </Button>
              </div>
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        control={form.control}
        name="endDate"
        render={({ field }) => (
          <FormItem className="space-y-1">
            <FormLabel htmlFor="endDate-picker">End Date</FormLabel>
            <FormControl>
              <div className="flex items-center space-x-2">
                <EnrichedDatePicker
                  selectedDate={field.value ? new Date(field.value) : undefined}
                  onDateAccepted={date =>
                    field.onChange(
                      date !== undefined ? localIsoDate(date) : undefined,
                    )
                  }
                  triggerClassName="flex-1"
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="w-16"
                  onClick={() => field.onChange(undefined)}
                >
                  Clear
                </Button>
              </div>
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <div className="flex space-x-4 items-baseline">
        <FormField
          control={form.control}
          name="frequencyCount"
          render={({ field }) => (
            <FormItem className="space-y-1 flex-1">
              <FormLabel htmlFor="frequencyCount-input">Every</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="frequencyCount-input"
                  type="number"
                  min={1}
                  className="w-full"
                  onChange={e => field.onChange(e.target.valueAsNumber)}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="frequency"
          render={({ field }) => (
            <FormItem className="space-y-1 flex-1">
              <FormLabel htmlFor="frequency-select">Frequency</FormLabel>
              <FormControl>
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger id="frequency-select" className="w-full">
                    <SelectValue placeholder="Select frequency" />
                  </SelectTrigger>
                  <SelectContent>
                    {FrequencyEnumValues.map(freq => (
                      <SelectItem key={freq} value={freq}>
                        {freq}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      <FormField
        control={form.control}
        name="accountRowId"
        render={({ field }) => (
          <FormItem className="space-y-1">
            <FormLabel htmlFor="account-select">Associated Account</FormLabel>
            <FormControl>
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger id="account-select" className="w-full">
                  <SelectValue placeholder="Select an account" />
                </SelectTrigger>
                <SelectContent>
                  {accounts.map(acc => (
                    <SelectItem key={acc.rowId} value={acc.rowId}>
                      {acc.description}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
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
