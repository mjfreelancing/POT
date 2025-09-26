import { zodResolver } from '@hookform/resolvers/zod';
import { Key } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import useChangePassword from '@/api/hooks/useChangePassword';
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
import { Skeleton } from '@/components/ui/skeleton';
import type { DisplayError } from '@/lib';
import { logger } from '@/lib/logging';

import {
  ChangePasswordFields,
  changePasswordSchema,
} from './changePasswordSchema';

function ChangePasswordForm() {
  const form = useForm<ChangePasswordFields>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
    mode: 'onSubmit',
  });

  const { changePassword, isPending } = useChangePassword();
  const [error, setError] = useState<DisplayError | null>(null);

  useEffect(() => {
    logger.info('ChangePasswordForm', 'Mounted');

    return () => {
      logger.info('ChangePasswordForm', 'Unmounted');
    };
  }, []);

  async function onSubmit(values: ChangePasswordFields) {
    const result = await changePassword({
      currentPassword: values.currentPassword,
      newPassword: values.newPassword,
    });

    if (result && !result.success) {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      return;
    }

    if (result && result.success) {
      toast(
        () => (
          <SuccessToast
            icon={Key}
            title="Password Changed"
            description="Your password was updated successfully."
          />
        ),
        { duration: 5000 },
      );
      form.reset();
    }
  }

  if (isPending) {
    return <Skeleton className="mb-6 px-4 py-3 h-64 w-full rounded-lg" />;
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
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
                  type="password"
                  autoComplete="current-password"
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
                  type="password"
                  autoComplete="new-password"
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
                  type="password"
                  autoComplete="new-password"
                  disabled={isPending}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full mt-2" disabled={isPending}>
          Change Password
        </Button>
      </form>
    </Form>
  );
}

export { ChangePasswordForm };
