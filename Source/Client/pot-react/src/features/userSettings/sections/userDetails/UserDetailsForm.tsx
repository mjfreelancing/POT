import { zodResolver } from '@hookform/resolvers/zod';
import { Key } from 'lucide-react';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import { useApiUpdateUser } from '@/api/hooks/useUser';
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
import { useUserStore } from '@/stores';

import type { UserDetailsFields } from './userDetailsSchema';
import { userDetailsSchema } from './userDetailsSchema';

function UserDetailsForm() {
  const { userInfo, setUserInfo } = useUserStore();
  const updateUser = useApiUpdateUser();
  const { error, setError } = useErrorContext();

  // Will not be null once the user has logged in - can't get here until then
  const userDetails = userInfo!;

  const form = useForm<UserDetailsFields>({
    resolver: zodResolver(userDetailsSchema),
    defaultValues: {
      username: userDetails.username,
      displayName: userDetails.displayName,
      email: userDetails.email,
    },
    mode: 'onSubmit',
  });

  useEffect(() => {
    logger.info('UserDetailsForm', 'Mounted');

    return () => {
      logger.info('UserDetailsForm', 'Unmounted');
    };
  }, []);

  useEffect(() => {
    form.reset({
      username: userDetails.username,
      displayName: userDetails.displayName,
      email: userDetails.email,
    });
  }, [userDetails, form]);

  async function onSubmit(values: UserDetailsFields) {
    const result = await updateUser.mutateAsync({
      id: userDetails.rowId,
      data: {
        displayName: values.displayName,
        email: values.email,
        etag: userDetails.etag,
      },
    });

    if (!result.success) {
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
            title="User Details Updated"
            description="Your user details were updated successfully."
          />
        ),
        { duration: 5000 },
      );

      form.reset();
    }

    setUserInfo({
      ...userDetails,
      etag: result.value.etag,
      displayName: values.displayName,
      email: values.email,
    });
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
          name="username"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="username">Username</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="username"
                  type="text"
                  disabled
                  className="bg-gray-100 cursor-not-allowed"
                  aria-description="Your unique username, cannot be changed"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="displayName"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="display-name">Display Name</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="display-name"
                  type="text"
                  aria-description="Your display name, visible to others"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="email">Email</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="email"
                  type="email"
                  aria-description="Your email address for communication"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full mt-2">
          Update User Details
        </Button>
      </form>
    </Form>
  );
}

export default UserDetailsForm;
