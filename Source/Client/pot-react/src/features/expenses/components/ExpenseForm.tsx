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
import { Switch } from '@/components/ui/switch';
import { Textarea } from '@/components/ui/textarea';
import type { Account } from '@/data';
import {
  dateIsoFormat,
  Frequency,
  FrequencyOptions,
  todayIsoFormat,
} from '@/lib';

import { ExpenseFormData } from '../schemas/expenseFormSchema';

type ExpenseFormProps = {
  form: UseFormReturn<ExpenseFormData>;
  onSubmit: (values: ExpenseFormData) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
  accounts: Account[];
  isEditMode?: boolean;
};

// Custom hook to manage pickerDate state for endDate
function useEndDatePicker(form: UseFormReturn<ExpenseFormData>) {
  const endDateValue = form.watch('endDate');
  const [pickerDate, setPickerDate] = useState<Date | undefined>(
    endDateValue ? new Date(endDateValue) : undefined,
  );

  // Keep local state in sync with form value
  useEffect(() => {
    setPickerDate(endDateValue ? new Date(endDateValue) : undefined);
  }, [endDateValue]);

  // Called when user accepts a date selection or clears the field
  function syncPickerDate(date: Date | undefined) {
    setPickerDate(date);

    form.setValue(
      'endDate',
      date !== undefined ? dateIsoFormat(date) : undefined,
      { shouldValidate: true },
    );
  }

  return { pickerDate, syncPickerDate };
}

function ExpenseForm({
  form,
  onSubmit,
  onCancel,
  submitLabel,
  accounts,
  isEditMode = false,
}: ExpenseFormProps) {
  const { pickerDate, syncPickerDate } = useEndDatePicker(form);

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
                  aria-description="A memorable name for this expense"
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
                  placeholder="Enter the expense amount"
                  aria-description="Current expense amount"
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
                    selectedDate={
                      field.value ? new Date(field.value) : undefined
                    }
                    onDateAccepted={date => {
                      const value =
                        date !== undefined ? dateIsoFormat(date) : undefined;
                      field.onChange(value);
                      form.trigger('nextDue');
                    }}
                    triggerClassName="flex-1"
                    triggerId="nextDue-picker"
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="w-16"
                    onClick={() => {
                      field.onChange(todayIsoFormat());
                      form.trigger('nextDue');
                    }}
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
          name="accrualStart"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="accrualStart-picker">Accrual Start</FormLabel>
              <FormControl>
                <div className="flex items-center space-x-2">
                  <EnrichedDatePicker
                    selectedDate={
                      field.value ? new Date(field.value) : undefined
                    }
                    onDateAccepted={date => {
                      const value =
                        date !== undefined ? dateIsoFormat(date) : undefined;
                      field.onChange(value);
                      form.trigger('accrualStart');
                    }}
                    triggerClassName="flex-1"
                    triggerId="accrualStart-picker"
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="w-16"
                    onClick={() => {
                      field.onChange(todayIsoFormat());
                      form.trigger('accrualStart');
                    }}
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
          render={() => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="endDate-picker">End Date</FormLabel>
              <FormControl>
                <div className="flex items-center space-x-2">
                  <EnrichedDatePicker
                    selectedDate={pickerDate}
                    onDateAccepted={syncPickerDate}
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
                  placeholder="Add any notes about this expense"
                  className="min-h-[80px] max-h-[200px]"
                  value={field.value ?? ''}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="excludeFromCalcs"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3 shadow-sm">
              <div className="space-y-0.5">
                <FormLabel htmlFor="exclude-from-calcs-switch">
                  Exclude from Calculations
                </FormLabel>
              </div>
              <FormControl>
                <Switch
                  id="exclude-from-calcs-switch"
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
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

export default ExpenseForm;
