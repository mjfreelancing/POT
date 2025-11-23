import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { Loader2, UserPlus } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';
import { toast } from 'sonner';

import { useRoles } from '@/api/hooks/useRoles';
import { useInviteUser } from '@/api/hooks/useUsers';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import { DialogDescription, DialogTitle } from '@/components/ui/dialog';
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
import { Sheet, SheetContent } from '@/components/ui/sheet';
import { logger, useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Role } from '@/data/role';

import {
  type UserInvitationFormData,
  userInvitationFormSchema,
} from '../schemas';

function InviteUserSheet() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const { error, setError } = useErrorContext();

  const form = useForm<UserInvitationFormData>({
    resolver: zodResolver(userInvitationFormSchema),
    defaultValues: {
      username: '',
      email: '',
      roleId: '',
    },
  });

  // Fetch available roles
  const rolesQuery = useRoles();
  const inviteUserMutation = useInviteUser();

  const roles: Role[] = rolesQuery.data?.success ? rolesQuery.data.value : [];
  const isLoadingRoles = rolesQuery.isLoading;

  // Show loading state if roles are still loading
  if (isLoadingRoles) {
    return <LoadingMessage />;
  }

  // Handle roles API errors - check for explicit failure
  if (rolesQuery.data && !rolesQuery.data.success) {
    return (
      <ErrorSheet
        title={rolesQuery.data.error.code}
        description={rolesQuery.data.error.description}
        onDismiss={() => navigate('/users')}
      />
    );
  }

  const onSubmit = async (data: UserInvitationFormData) => {
    logger.info('InviteUserSheet', 'Submitting invitation', {
      username: data.username,
      email: data.email,
    });

    // Transform single roleId to roleIds array for server API
    const invitationData = {
      username: data.username,
      email: data.email,
      roleIds: [data.roleId],
    };

    const result = await inviteUserMutation.mutateAsync({
      data: invitationData,
    });

    if (result.success) {
      // Invalidate users cache to show the pending invite
      invalidateCache(['users']);

      toast(
        <SuccessToast
          icon={UserPlus}
          title="Invitation Sent"
          description={`User ${data.username} has been invited successfully.`}
        />,
      );

      // Navigate back to users page on success
      navigate('/users');
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <Sheet open modal={false}>
      <SheetContent className="p-6 sm:max-w-lg [&>button:first-of-type]:hidden">
        <div className="space-y-6 pr-6 pl-6">
          <DialogTitle className="text-lg font-semibold">
            Invite New User
          </DialogTitle>
          <DialogDescription className="sr-only">
            Invite New User form
          </DialogDescription>
          <Separator />

          {error && (
            <ErrorSheet
              title={error.title}
              description={error.description}
              onDismiss={() => setError(null)}
            />
          )}

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <FormField
                control={form.control}
                name="username"
                render={({ field }) => (
                  <FormItem className="space-y-1">
                    <FormLabel>Username</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter your unique username"
                        {...field}
                        disabled={inviteUserMutation.isPending}
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
                    <FormLabel>Email Address</FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder="Enter your email address"
                        {...field}
                        disabled={inviteUserMutation.isPending}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="roleId"
                render={({ field }) => (
                  <FormItem className="space-y-1">
                    <FormLabel>Assign Role</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                      disabled={inviteUserMutation.isPending || isLoadingRoles}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select a role" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {roles.length > 0 ? (
                          roles.map(role => (
                            <SelectItem key={role.rowId} value={role.rowId}>
                              {role.name}
                            </SelectItem>
                          ))
                        ) : (
                          <SelectItem value="" disabled>
                            No roles available
                          </SelectItem>
                        )}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="space-y-4 pt-2">
                <Separator className="opacity-80" />
                <div className="flex justify-end space-x-2">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => navigate('/users')}
                    disabled={inviteUserMutation.isPending}
                  >
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    disabled={
                      inviteUserMutation.isPending || roles.length === 0
                    }
                  >
                    {inviteUserMutation.isPending && (
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    )}
                    Send Invitation
                  </Button>
                </div>
              </div>
            </form>
          </Form>
        </div>
      </SheetContent>
    </Sheet>
  );
}

export default InviteUserSheet;
