import { zodResolver } from '@hookform/resolvers/zod';
import { Key } from 'lucide-react';
import { useEffect } from 'react';
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
import { useErrorContext } from '@/contexts';
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
  const { error, setError } = useErrorContext();

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

        <Button type="submit" className="w-full mt-2" disabled={isPending}>
          Change Password
        </Button>
      </form>
    </Form>
  );
}

export default ChangePasswordForm;
