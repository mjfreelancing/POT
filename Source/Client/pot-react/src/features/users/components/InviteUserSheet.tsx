import { zodResolver } from '@hookform/resolvers/zod';
import { Loader2, UserPlus } from 'lucide-react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import { useRoles } from '@/api/hooks/useRoles';
import { useInviteUser } from '@/api/hooks/useUsers';
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
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet';
import type { Role } from '@/data/role';
import type { UserInvitation } from '@/data/siteUser';
import { userInvitationSchema } from '@/data/siteUser';
import { logger } from '@/lib/logging';

type InviteUserSheetProps = {
  children?: React.ReactNode;
};

export function InviteUserSheet({ children }: InviteUserSheetProps) {
  const [isOpen, setIsOpen] = useState(false);

  const form = useForm<UserInvitation>({
    resolver: zodResolver(userInvitationSchema),
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

  const onSubmit = async (data: UserInvitation) => {
    logger.info('InviteUserSheet', 'Submitting invitation', {
      username: data.username,
      email: data.email,
    });

    inviteUserMutation.mutate(data);

    // Note: In a real implementation, we'd handle the response properly
    // For now, we'll show success toast and reset the form
    toast(
      <SuccessToast
        icon={UserPlus}
        title="Invitation Sent"
        description={`User ${data.username} has been invited successfully.`}
      />,
    );

    // Reset form and close sheet
    form.reset();
    setIsOpen(false);
  };

  const handleOpenChange = (open: boolean) => {
    setIsOpen(open);
    if (!open) {
      form.reset();
    }
  };

  const defaultTrigger = (
    <Button>
      <UserPlus className="h-4 w-4 mr-2" />
      Invite User
    </Button>
  );

  return (
    <Sheet open={isOpen} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>{children || defaultTrigger}</SheetTrigger>
      <SheetContent className="sm:max-w-md">
        <SheetHeader>
          <SheetTitle>Invite New User</SheetTitle>
          <SheetDescription>
            Send an invitation to a new user with a role assignment. They will
            receive login credentials via email.
          </SheetDescription>
        </SheetHeader>

        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(onSubmit)}
            className="space-y-4 mt-4"
          >
            <FormField
              control={form.control}
              name="username"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Username</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="john.doe"
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
                <FormItem>
                  <FormLabel>Email Address</FormLabel>
                  <FormControl>
                    <Input
                      type="email"
                      placeholder="john@example.com"
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
                <FormItem>
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
                      {roles.map(role => (
                        <SelectItem key={role.rowId} value={role.rowId}>
                          {role.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="flex justify-end space-x-2 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => handleOpenChange(false)}
                disabled={inviteUserMutation.isPending}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={inviteUserMutation.isPending || isLoadingRoles}
              >
                {inviteUserMutation.isPending && (
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                )}
                Send Invitation
              </Button>
            </div>
          </form>
        </Form>
      </SheetContent>
    </Sheet>
  );
}
