import { zodResolver } from '@hookform/resolvers/zod';
import { forwardRef, useEffect, useImperativeHandle, useState } from 'react';
import { useForm } from 'react-hook-form';

import useChangePassword from '@/api/hooks/useChangePassword';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
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
import { logger, logoutManager } from '@/concerns';
import { useErrorContext } from '@/contexts';

import type {
  SettingsSectionFormHandle,
  SettingsSectionFormProps,
  SettingsSectionFormSubmitResult,
} from '../settingsSectionForm';
import type { ChangePasswordFields } from './changePasswordSchema';
import { changePasswordSchema } from './changePasswordSchema';
import PasswordChangedDialog from './PasswordChangedDialog';

type ChangePasswordFormProps = SettingsSectionFormProps;

const ChangePasswordForm = forwardRef<
  SettingsSectionFormHandle,
  ChangePasswordFormProps
>(function ChangePasswordForm({ onDirtyChange }: ChangePasswordFormProps, ref) {
  const [showPasswordChangedDialog, setShowPasswordChangedDialog] =
    useState(false);
  const form = useForm<ChangePasswordFields>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
    mode: 'onSubmit',
  });
  const isDirty = form.formState.isDirty;

  const { changePassword, isPending } = useChangePassword();
  const { error, setError } = useErrorContext();

  useEffect(() => {
    logger.info('ChangePasswordForm', 'Mounted');

    return () => {
      logger.info('ChangePasswordForm', 'Unmounted');
    };
  }, []);

  useEffect(() => {
    onDirtyChange?.(isDirty);
  }, [isDirty, onDirtyChange]);

  async function onSubmit(
    values: ChangePasswordFields,
  ): Promise<SettingsSectionFormSubmitResult> {
    // Clear any previous errors
    setError(null);

    const result = await changePassword({
      currentPassword: values.currentPassword,
      newPassword: values.newPassword,
    });

    if (result && !result.success) {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      return 'invalid';
    }

    if (result && result.success) {
      // Show modal dialog instead of toast
      form.reset({
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
      });
      setShowPasswordChangedDialog(true);

      return 'blocked';
    }

    return 'invalid';
  }

  function handleLogoutAfterPasswordChange() {
    logoutManager.logout();
  }

  useImperativeHandle(
    ref,
    () => ({
      submit: async () => {
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
          currentPassword: '',
          newPassword: '',
          confirmPassword: '',
        });
      },
    }),
    [form, onSubmit, setError],
  );

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(async values => {
          await onSubmit(values);
        })}
        className="space-y-6"
      >
        {error && (
          <ErrorSheet
            title={error.title}
            description={error.description}
            onDismiss={() => setError(null)}
          />
        )}
        <FormField
          control={form.control}
          name="currentPassword"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="current-password">Current Password</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="current-password"
                  name="current-password"
                  autoComplete="current-password"
                  type="password"
                  placeholder="Enter your current password"
                  disabled={isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="new-password">New Password</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="new-password"
                  name="new-password"
                  autoComplete="new-password"
                  type="password"
                  placeholder="Enter your new password"
                  disabled={isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="confirm-password">
                Confirm New Password
              </FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="confirm-password"
                  name="confirm-password"
                  autoComplete="confirm-password"
                  type="password"
                  placeholder="Enter your new password again"
                  disabled={isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="text-xs text-muted-foreground space-y-1 pt-2">
          <p>Password must contain:</p>
          <ul className="list-disc list-inside space-y-0.5 ml-2">
            <li>At least one uppercase letter</li>
            <li>At least one lowercase letter</li>
            <li>At least one digit</li>
            <li>At least one special character</li>
          </ul>
        </div>

        <Button type="submit" className="w-full mt-2" disabled={!isDirty || isPending}>
          Change Password
        </Button>
      </form>

      <PasswordChangedDialog
        open={showPasswordChangedDialog}
        onLogout={handleLogoutAfterPasswordChange}
      />
    </Form>
  );
});

export default ChangePasswordForm;
