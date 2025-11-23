import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { Loader2, RotateCcw } from 'lucide-react';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import { useRoles } from '@/api/hooks/useRoles';
import { useUpdateUserRole } from '@/api/hooks/useUsers';
import { ErrorSheet } from '@/components/feedback';
import { SuccessToast } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { logger, useCacheInvalidation } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Role } from '@/data/role';
import type { SiteUser } from '@/data/siteUser';

import {
  type UserRoleUpdateFormData,
  userRoleUpdateFormSchema,
} from '../schemas';

type UserRoleDialogProps = {
  user: SiteUser | null;
  isOpen: boolean;
  onClose: () => void;
};

export function UserRoleDialog({ user, isOpen, onClose }: UserRoleDialogProps) {
  const form = useForm<UserRoleUpdateFormData>({
    resolver: zodResolver(userRoleUpdateFormSchema),
    defaultValues: {
      roleId: '',
    },
  });

  // Fetch available roles
  const rolesQuery = useRoles();
  const updateRoleMutation = useUpdateUserRole();
  const queryClient = useQueryClient();
  const invalidateCache = useCacheInvalidation(queryClient);
  const { error, setError } = useErrorContext();

  const roles: Role[] = rolesQuery.data?.success ? rolesQuery.data.value : [];
  const isLoadingRoles = rolesQuery.isLoading;

  // Find current role ID from role name
  const currentRole = user
    ? roles.find(role => user.roles.includes(role.name))
    : null;

  // Filter out current role from available options
  const availableRoles = roles.filter(
    role => role.rowId !== currentRole?.rowId,
  );

  const onSubmit = async (data: UserRoleUpdateFormData) => {
    if (!user) return;

    logger.info('UserRoleDialog', 'Updating user role', {
      userId: user.rowId,
      username: user.username,
      newRoleId: data.roleId,
    });

    // Transform single roleId to roleIds array for server API
    // API expects a complete replacement array of role ids with current ETag
    const result = await updateRoleMutation.mutateAsync({
      id: user.rowId,
      data: {
        etag: user.etag,
        roleIds: [data.roleId],
      },
    });

    if (result.success) {
      invalidateCache(['users']);

      const selectedRole = roles.find(role => role.rowId === data.roleId);

      toast(
        <SuccessToast
          icon={RotateCcw}
          title="Role Updated"
          description={`${user.username}'s role has been changed to ${selectedRole?.name}.`}
        />,
        { duration: 5000 },
      );

      // Close dialog only on success
      onClose();
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });

      logger.error(
        'UserRoleDialog',
        'Failed to update user role',
        result.error,
      );
    }
  };

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      onClose();
      form.reset();
    }
  };

  // Reset form when dialog opens or user changes
  useEffect(() => {
    if (isOpen && user) {
      form.reset({ roleId: '' });
    }
  }, [isOpen, user, form]);

  if (!user) return null;

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <Dialog open={isOpen} onOpenChange={handleOpenChange}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <div className="flex items-center gap-3">
              <DialogTitle>Change Role</DialogTitle>
              <div className="flex items-center gap-1">
                {user.roles.length > 0 ? (
                  user.roles.map((role, index) => (
                    <span
                      key={index}
                      className="inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ring-1 ring-inset min-w-[80px] justify-center bg-purple-100 text-purple-800 ring-purple-600/20 dark:bg-purple-900/30 dark:text-purple-300 dark:ring-purple-400/30"
                    >
                      {role}
                    </span>
                  ))
                ) : (
                  <span className="inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ring-1 ring-inset min-w-[80px] justify-center bg-gray-100 text-gray-600 ring-gray-600/20 dark:bg-gray-800 dark:text-gray-400 dark:ring-gray-400/30">
                    No role assigned
                  </span>
                )}
              </div>
            </div>
            <DialogDescription>
              Update the role assignment for{' '}
              <span className="font-medium text-foreground">
                {user.username}
              </span>
            </DialogDescription>
          </DialogHeader>

          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(onSubmit)}
              className="space-y-4 mt-4"
            >
              <FormField
                control={form.control}
                name="roleId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium">
                      New Role
                    </FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={updateRoleMutation.isPending || isLoadingRoles}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select a role" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {availableRoles.length > 0 ? (
                          availableRoles.map(role => (
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
                <DialogFooter className="gap-4">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => handleOpenChange(false)}
                    disabled={updateRoleMutation.isPending}
                    className="w-28"
                  >
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    disabled={
                      updateRoleMutation.isPending ||
                      isLoadingRoles ||
                      availableRoles.length === 0
                    }
                    className="w-32"
                  >
                    {updateRoleMutation.isPending && (
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    )}
                    Update Role
                  </Button>
                </DialogFooter>
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </>
  );
}
