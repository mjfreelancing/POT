import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { PiggyBank } from 'lucide-react';
import { forwardRef, useEffect, useImperativeHandle } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import {
  useApiGetSettings,
  useApiUpdateSetting,
} from '@/api/hooks/useSettings';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { SuccessToast } from '@/components/feedback/toast';
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
import { logger, useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type {
  SettingsSectionFormHandle,
  SettingsSectionFormProps,
  SettingsSectionFormSubmitResult,
} from '@/features/userSettings/sections/settingsSectionForm';

import type {
  BudgetRemindersFields,
  BudgetRemindersFormValues,
} from './budgetRemindersSchema';
import { budgetRemindersSchema } from './budgetRemindersSchema';

type BudgetRemindersFormProps = SettingsSectionFormProps & {
  readonly?: boolean;
};

type ReminderSettingKey = 'Enabled' | 'ReminderDays' | 'LocalHourTrigger';

type ReminderSettingRecord = {
  rowId?: string;
  etag?: bigint;
  value: boolean | number;
};

const EMAIL_BUDGET_REMINDER_CATEGORY = 'EmailBudgetReminder';

// Fallback values used when the settings row does not yet exist in the database.
// These must match the server-side defaults in SettingsService.GetEmailBudgetReminderSettingsAsync.
const DEFAULT_ENABLED = false;
const DEFAULT_REMINDER_DAYS = 7;
const DEFAULT_LOCAL_HOUR_TRIGGER = 6; // 6 AM

const HOUR_OPTIONS = Array.from({ length: 24 }, (_, hour) => ({
  value: hour,
  label: formatHourLabel(hour),
}));

const BudgetRemindersForm = forwardRef<
  SettingsSectionFormHandle,
  BudgetRemindersFormProps
>(function BudgetRemindersForm(
  { readonly = false, onDirtyChange }: BudgetRemindersFormProps,
  ref,
) {
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const settingsQuery = useApiGetSettings();
  const updateSetting = useApiUpdateSetting();
  const { error, setError } = useErrorContext();

  // Derive primitive values from the server response so effect dependency arrays
  // contain stable primitive values rather than the settings object. Using the
  // object directly would cause the hydration effect to run on every render
  // because a new object reference is created each time the query resolves.
  const reminderSettings = getBudgetReminderSettings(settingsQuery.data);
  const hasReminderSettings = reminderSettings !== null;
  const enabledValue = asBoolean(
    reminderSettings?.Enabled?.value,
    DEFAULT_ENABLED,
  );
  const reminderDaysValue = asNumber(
    reminderSettings?.ReminderDays?.value,
    DEFAULT_REMINDER_DAYS,
  );
  const localHourTriggerValue = asNumber(
    reminderSettings?.LocalHourTrigger?.value,
    DEFAULT_LOCAL_HOUR_TRIGGER,
  );

  // defaultValues seed the form on first mount. After that, the hydration
  // effect below is responsible for syncing server values back into the form
  // whenever they change and the form is not dirty.
  const form = useForm<
    BudgetRemindersFormValues,
    undefined,
    BudgetRemindersFields
  >({
    resolver: zodResolver(budgetRemindersSchema),
    defaultValues: {
      enabled: enabledValue,
      reminderDays: reminderDaysValue,
      localHourTrigger: localHourTriggerValue,
    },
    mode: 'onSubmit',
  });
  const isDirty = form.formState.isDirty;

  useEffect(() => {
    logger.info('BudgetRemindersForm', 'Mounted');

    return () => {
      logger.info('BudgetRemindersForm', 'Unmounted');
    };
  }, []);

  // Hydration effect: keeps the form in sync with the server-side values after
  // background refetches or cache invalidations. Only runs when the form is
  // clean — if the user has unsaved edits, the incoming server values are
  // ignored so local changes are never silently overwritten.
  useEffect(() => {
    if (!hasReminderSettings || isDirty) {
      return;
    }

    form.reset({
      enabled: enabledValue,
      reminderDays: reminderDaysValue,
      localHourTrigger: localHourTriggerValue,
    });
  }, [
    enabledValue,
    form,
    hasReminderSettings,
    isDirty,
    localHourTriggerValue,
    reminderDaysValue,
  ]);

  // Propagates React Hook Form's (RHF) isDirty flag to the parent sheet via the onDirtyChange
  // callback. The readonly guard ensures a read-only form never reports itself
  // as dirty, which would incorrectly trigger the unsaved-changes prompt.
  useEffect(() => {
    onDirtyChange?.(!readonly && isDirty);
  }, [isDirty, onDirtyChange, readonly]);

  async function onSubmit(
    values: BudgetRemindersFields,
  ): Promise<SettingsSectionFormSubmitResult> {
    setError(null);

    const settingUpdates: { key: ReminderSettingKey; value: string }[] = [
      { key: 'Enabled', value: String(values.enabled) },
      { key: 'ReminderDays', value: String(values.reminderDays) },
      { key: 'LocalHourTrigger', value: String(values.localHourTrigger) },
    ];

    let latestEnabled = reminderSettings?.Enabled;
    let latestReminderDays = reminderSettings?.ReminderDays;
    let latestLocalHourTrigger = reminderSettings?.LocalHourTrigger;

    for (const settingUpdate of settingUpdates) {
      const existingSetting =
        settingUpdate.key === 'Enabled'
          ? latestEnabled
          : settingUpdate.key === 'ReminderDays'
            ? latestReminderDays
            : latestLocalHourTrigger;

      const result = await updateSetting.mutateAsync({
        id: `${EMAIL_BUDGET_REMINDER_CATEGORY}/${settingUpdate.key}`,
        data: {
          value: settingUpdate.value,
          etag: existingSetting?.etag ?? null,
        },
      });

      if (!result.success) {
        setError({
          title: result.error.code,
          description: result.error.description,
        });

        return 'invalid';
      }

      const nextSetting: ReminderSettingRecord = {
        rowId: result.value.rowId,
        etag: result.value.etag,
        value:
          settingUpdate.key === 'Enabled'
            ? values.enabled
            : settingUpdate.key === 'ReminderDays'
              ? values.reminderDays
              : values.localHourTrigger,
      };

      if (settingUpdate.key === 'Enabled') {
        latestEnabled = nextSetting;
      } else if (settingUpdate.key === 'ReminderDays') {
        latestReminderDays = nextSetting;
      } else {
        latestLocalHourTrigger = nextSetting;
      }
    }

    await queryClient.invalidateQueries({ queryKey: ['settings'] });
    invalidateCache(['me']);
    form.reset({
      enabled: values.enabled,
      reminderDays: values.reminderDays,
      localHourTrigger: values.localHourTrigger,
    });

    toast(
      () => (
        <SuccessToast
          icon={PiggyBank}
          title="Budget Reminders Updated"
          description="Your budget reminder settings were updated successfully."
        />
      ),
      { duration: 5000 },
    );

    return 'saved';
  }

  const isPending = settingsQuery.isLoading || updateSetting.isPending;

  // Exposes submit() and discard() to the parent sheet via the forwarded ref.
  //
  // submit()  — delegates to RHF handleSubmit(), which validates first. On
  //             success it calls onSubmit() and returns its result ('saved' or
  //             'invalid'). On validation failure it short-circuits to 'invalid'.
  //             Returns 'blocked' immediately when in readonly mode.
  //
  // discard() — resets the form to the last known server values (the primitives
  //             derived above) and clears any visible API error. RHF will mark
  //             the form as clean after reset, which triggers the dirty-change
  //             effect and notifies the parent sheet.
  useImperativeHandle(
    ref,
    () => ({
      submit: async () => {
        if (readonly) {
          return 'blocked';
        }

        let submitResult: SettingsSectionFormSubmitResult = 'invalid';

        await form.handleSubmit(
          async values => {
            submitResult = await onSubmit(values);
          },
          async () => {
            submitResult = 'invalid';
          },
        )();

        return submitResult;
      },
      discard: () => {
        setError(null);
        form.reset({
          enabled: enabledValue,
          reminderDays: reminderDaysValue,
          localHourTrigger: localHourTriggerValue,
        });
      },
    }),
    [
      enabledValue,
      form,
      localHourTriggerValue,
      onSubmit,
      readonly,
      reminderDaysValue,
      setError,
    ],
  );

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(async values => {
          await onSubmit(values);
        })}
        className="space-y-6"
        noValidate
      >
        {readonly && (
          <div className="inline-flex rounded-md border border-border/50 bg-amber-500/10 px-2 py-1 text-xs text-amber-700 dark:text-amber-400">
            View only
          </div>
        )}

        {error && (
          <ErrorSheet
            title={error.title}
            description={error.description}
            onDismiss={() => setError(null)}
          />
        )}

        <FormField
          control={form.control}
          name="enabled"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3 shadow-sm">
              <div className="space-y-0.5">
                <FormLabel htmlFor="budget-reminders-enabled">
                  Enable Budget Reminders
                </FormLabel>
              </div>
              <FormControl>
                <Switch
                  id="budget-reminders-enabled"
                  checked={Boolean(field.value)}
                  onCheckedChange={field.onChange}
                  disabled={readonly || isPending}
                />
              </FormControl>
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="reminderDays"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="budget-reminders-days">
                Advance Notice (Days)
              </FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="budget-reminders-days"
                  type="number"
                  step={1}
                  value={typeof field.value === 'number' ? field.value : ''}
                  onChange={event => field.onChange(event.target.valueAsNumber)}
                  disabled={readonly || isPending}
                  aria-description="Number of days before the budget period to send the reminder"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="localHourTrigger"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="budget-reminders-hour">
                Reminder Time
              </FormLabel>
              <FormControl>
                <Select
                  value={
                    typeof field.value === 'number' ? String(field.value) : ''
                  }
                  onValueChange={value => {
                    if (value === '') {
                      return;
                    }

                    field.onChange(Number(value));
                  }}
                  disabled={readonly || isPending}
                >
                  <SelectTrigger
                    id="budget-reminders-hour"
                    className="w-full"
                    aria-description="Local time of day to send the reminder"
                  >
                    <SelectValue placeholder="Select reminder time" />
                  </SelectTrigger>
                  <SelectContent>
                    {HOUR_OPTIONS.map(option => (
                      <SelectItem
                        key={option.value}
                        value={String(option.value)}
                      >
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {!readonly && (
          <Button
            type="submit"
            className="w-full mt-2"
            disabled={!isDirty || isPending}
          >
            Update Budget Reminders
          </Button>
        )}
      </form>
    </Form>
  );
});

function getBudgetReminderSettings(
  result: ReturnType<typeof useApiGetSettings>['data'],
): Record<ReminderSettingKey, ReminderSettingRecord | undefined> | null {
  if (!result || !result.success) {
    return null;
  }

  const filteredSettings = result.value.settings.filter(
    setting => setting.category === EMAIL_BUDGET_REMINDER_CATEGORY,
  );

  return {
    Enabled: toReminderSettingRecord(
      filteredSettings.find(setting => setting.key === 'Enabled'),
    ),
    ReminderDays: toReminderSettingRecord(
      filteredSettings.find(setting => setting.key === 'ReminderDays'),
    ),
    LocalHourTrigger: toReminderSettingRecord(
      filteredSettings.find(setting => setting.key === 'LocalHourTrigger'),
    ),
  };
}

function toReminderSettingRecord(
  setting:
    | {
        rowId?: string;
        etag?: bigint;
        value: boolean | number | string;
      }
    | undefined,
): ReminderSettingRecord | undefined {
  if (!setting) {
    return undefined;
  }

  return {
    rowId: setting.rowId,
    etag: setting.etag,
    value: normalizeReminderSettingValue(setting.value),
  };
}

function normalizeReminderSettingValue(
  value: boolean | number | string,
): boolean | number {
  if (typeof value === 'boolean' || typeof value === 'number') {
    return value;
  }

  if (value === 'true') {
    return true;
  }

  if (value === 'false') {
    return false;
  }

  const parsedNumber = Number(value);

  if (!Number.isNaN(parsedNumber)) {
    return parsedNumber;
  }

  return 0;
}

function asBoolean(
  value: boolean | number | undefined,
  fallbackValue: boolean,
): boolean {
  if (typeof value === 'boolean') {
    return value;
  }

  return fallbackValue;
}

function asNumber(
  value: boolean | number | undefined,
  fallbackValue: number,
): number {
  if (typeof value === 'number') {
    return value;
  }

  return fallbackValue;
}

function formatHourLabel(hour: number): string {
  const suffix = hour < 12 ? 'AM' : 'PM';
  const hour12 = hour % 12 === 0 ? 12 : hour % 12;

  return `${hour12}:00 ${suffix}`;
}

export default BudgetRemindersForm;
