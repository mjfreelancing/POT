import { useEffect, useState } from 'react';
import { UseFormReturn } from 'react-hook-form';

import { MoneyValueChangeEvent, MoneyValueInput } from '@/components/input';
import { EnrichedDatePicker } from '@/components/picker';
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
import { Textarea } from '@/components/ui/textarea';
import type { Account } from '@/data';
import {
  dateIsoFormat,
  Frequency,
  FrequencyOptions,
  todayIsoFormat,
} from '@/lib';

import { IncomeFormData } from '../schemas/incomeFormSchema';

type IncomeFormProps = {
  form: UseFormReturn<IncomeFormData>;
  onSubmit: (values: IncomeFormData) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
  accounts: Account[];
  isEditMode?: boolean;
};

function IncomeForm({
  form,
  onSubmit,
  onCancel,
  submitLabel,
  accounts,
  isEditMode = false,
}: IncomeFormProps) {
  useEffect(() => {
    if (isEditMode) {
      form.setFocus('amount');
    }
  }, [isEditMode, form]);

  // Watch for frequency changes to handle One Time frequency
  useEffect(() => {
    const subscription = form.watch((value, { name }) => {
      if (name === 'frequency' && value.frequency === Frequency.OneTime) {
        form.setValue('frequencyCount', 0);
      }
    });

    return () => subscription.unsubscribe();
  }, [form]);

  return (
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
                  placeholder="Enter a description"
                  aria-description="A memorable name for this income"
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
                  {/* Two-way binding between form and picker:
                    1. selectedDate prop (from field.value) drives the picker's internal state via useEffect in EnrichedCalendar.
                    2. onDateChange (day clicks) pushes immediate date selections back to the form via field.onChange.
                    3. onDateAccepted (Accept button) commits the chosen date into the form.
                    4. Clear button calls field.onChange(undefined), which updates selectedDate to undefined,
                       triggering the picker's useEffect to reset its internal pickerDate to cleared state. */}
                  <EnrichedDatePicker
                    selectedDate={
                      field.value ? new Date(field.value) : undefined
                    }
                    onDateAccepted={date =>
                      field.onChange(
                        date !== undefined ? dateIsoFormat(date) : undefined,
                      )
                    }
                    onDateChange={date =>
                      field.onChange(
                        date !== undefined ? dateIsoFormat(date) : undefined,
                      )
                    }
                    triggerClassName="flex-1"
                    triggerId="nextDue-picker"
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="w-16"
                    onClick={() => field.onChange(todayIsoFormat())}
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
          render={({ field }) => {
            // React Hook Form does not always trigger a re-render of the field render function when the value changes from a defined date to undefined (or vice versa),
            // especially in edit mode or when clearing a field. This can cause the date picker UI to become out of sync with the form state, so the picker may not clear
            // even though the form value is cleared. To solve this, we use local state (pickerDate) to control the picker UI, initializing it from the form value and
            // keeping it in sync with form changes. When the user clears the field, we update both the form and local state, ensuring the picker UI always reflects the
            // cleared state regardless of React Hook Form's render optimizations.
            const [pickerDate, setPickerDate] = useState<Date | undefined>(
              field.value ? new Date(field.value) : undefined,
            );

            // Helper to sync picker and form state
            function syncPickerDate(date: Date | undefined) {
              setPickerDate(date);
              field.onChange(
                date !== undefined ? dateIsoFormat(date) : undefined,
              );
            }

            // Keep local state in sync with form value (for edit mode, or programmatic changes)
            useEffect(() => {
              const formDate = field.value ? new Date(field.value) : undefined;
              setPickerDate(formDate);
            }, [field.value]);

            return (
              <FormItem className="space-y-1">
                <FormLabel htmlFor="endDate-picker">End Date</FormLabel>
                <FormControl>
                  <div className="flex items-center space-x-2">
                    {/* Two-way binding between form and picker:
                      1. pickerDate state drives the picker's internal state.
                      2. onDateChange (day clicks) pushes immediate date selections back to the form and local state.
                      3. onDateAccepted (Accept button) commits the chosen date into the form and local state.
                      4. Clear button sets both form and local state to undefined, resetting the picker UI. */}
                    <EnrichedDatePicker
                      selectedDate={pickerDate}
                      onDateAccepted={syncPickerDate}
                      onDateChange={syncPickerDate}
                      triggerClassName="flex-1"
                      triggerId="endDate-picker"
                    />
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="w-16"
                      onClick={() => syncPickerDate(undefined)}
                    >
                      Clear
                    </Button>
                  </div>
                </FormControl>
                <FormMessage />
              </FormItem>
            );
          }}
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
                    min={0}
                    className="w-full"
                    onChange={e => field.onChange(e.target.valueAsNumber)}
                    disabled={form.watch('frequency') === Frequency.OneTime}
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
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    name={field.name}
                  >
                    <SelectTrigger id="frequency-select" className="w-full">
                      <SelectValue placeholder="Select frequency" />
                    </SelectTrigger>
                    <SelectContent>
                      {FrequencyOptions.map(({ value, label }) => (
                        <SelectItem key={value} value={value}>
                          {label}
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
                <Select
                  value={field.value}
                  onValueChange={field.onChange}
                  name={field.name}
                >
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

        <FormField
          control={form.control}
          name="note"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="note-textarea">Note (optional)</FormLabel>
              <FormControl>
                <Textarea
                  {...field}
                  id="note-textarea"
                  placeholder="Add any notes about this income"
                  className="min-h-[80px] max-h-[200px]"
                  value={field.value ?? ''}
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
}

export default IncomeForm;
