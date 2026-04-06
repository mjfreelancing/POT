import { useEffect, useState } from 'react';
import type { UseFormReturn } from 'react-hook-form';

import FormDateActionRow from '@/components/form/FormDateActionRow';
import FormLabeledItem from '@/components/form/FormLabeledItem';
import type { MoneyValueChangeEvent } from '@/components/input';
import { MoneyValueInput } from '@/components/input';
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
import { Separator } from '@/components/ui/separator';
import { Switch } from '@/components/ui/switch';
import { Textarea } from '@/components/ui/textarea';
import type { Account } from '@/data';
import {
  dateIsoFormat,
  FORM_SHEET_STYLES,
  Frequency,
  FrequencyOptions,
  todayIsoFormat,
} from '@/lib';

import type { IncomeFormData } from '../schemas/incomeFormSchema';

type IncomeFormProps = {
  form: UseFormReturn<IncomeFormData>;
  onSubmit: (values: IncomeFormData) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
  accounts: Account[];
  isEditMode?: boolean;
};

// Custom hook to manage pickerDate state for endDate
function useEndDatePicker(form: UseFormReturn<IncomeFormData>) {
  const endDateValue = form.watch('endDate');
  const [pickerDate, setPickerDate] = useState<Date | undefined>(
    endDateValue ? new Date(endDateValue) : undefined,
  );

  // Keep local state in sync with form value
  useEffect(() => {
    setPickerDate(endDateValue ? new Date(endDateValue) : undefined);
  }, [endDateValue]);

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

function IncomeForm({
  form,
  onSubmit,
  onCancel,
  submitLabel,
  accounts,
  isEditMode = false,
}: IncomeFormProps) {
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
            <FormLabeledItem label="Description" htmlFor="description-input">
                <Input
                  {...field}
                  id="description-input"
                  placeholder="Enter a description"
                  aria-description="A memorable name for this income"
                />
            </FormLabeledItem>
          )}
        />

        <FormField
          control={form.control}
          name="amount"
          render={({ field }) => (
            <FormLabeledItem label="Amount" htmlFor="amount-input">
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
            </FormLabeledItem>
          )}
        />

        <FormField
          control={form.control}
          name="nextDue"
          render={({ field }) => (
            <FormLabeledItem label="Next Due" htmlFor="nextDue-picker">
              <FormDateActionRow
                selectedDate={field.value ? new Date(field.value) : undefined}
                onDateAccepted={date => {
                  const value =
                    date !== undefined ? dateIsoFormat(date) : undefined;
                  field.onChange(value);
                  form.trigger('nextDue');
                }}
                triggerId="nextDue-picker"
                actionLabel="Today"
                onActionClick={() => {
                  field.onChange(todayIsoFormat());
                  form.trigger('nextDue');
                }}
              />
            </FormLabeledItem>
          )}
        />

        <FormField
          control={form.control}
          name="endDate"
          render={() => (
            <FormLabeledItem label="End Date" htmlFor="endDate-picker">
              <FormDateActionRow
                selectedDate={pickerDate}
                onDateAccepted={syncPickerDate}
                triggerId="endDate-picker"
                actionLabel="Clear"
                onActionClick={() => syncPickerDate(undefined)}
              />
            </FormLabeledItem>
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
            <FormLabeledItem
              label="Associated Account"
              htmlFor="account-select"
            >
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
            </FormLabeledItem>
          )}
        />

        <FormField
          control={form.control}
          name="note"
          render={({ field }) => (
            <FormLabeledItem label="Note (optional)" htmlFor="note-textarea">
                <Textarea
                  {...field}
                  id="note-textarea"
                  placeholder="Add any notes about this income"
                  className="min-h-[80px] max-h-[200px] text-sm sm:text-[13px]"
                  value={field.value ?? ''}
                />
            </FormLabeledItem>
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

export default IncomeForm;
