import { useForm } from 'react-hook-form';
import {
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';

type ChangePasswordFields = {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
};

function ChangePasswordForm() {
  const form = useForm<ChangePasswordFields>({
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
    mode: 'onSubmit',
  });

  function onSubmit(values: ChangePasswordFields) {
    // Placeholder for submit logic
    // TODO: Implement password change
    // eslint-disable-next-line no-console
    console.log(values);
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="currentPassword"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel>Current Password</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="current-password"
                  type="password"
                  autoComplete="current-password"
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
              <FormLabel>New Password</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="new-password"
                  type="password"
                  autoComplete="new-password"
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
              <FormLabel>Confirm New Password</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="confirm-password"
                  type="password"
                  autoComplete="new-password"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button type="submit" className="w-full mt-2">
          Change Password
        </Button>
      </form>
    </Form>
  );
}

export { ChangePasswordForm };
