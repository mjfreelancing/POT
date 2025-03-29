import { zodResolver } from '@hookform/resolvers/zod';
import { useRef } from 'react';
import { ControllerRenderProps, useForm } from 'react-hook-form';
import { z } from 'zod';

import { useUpdateAccountMutation } from '@/api/accounts/accountsApi';
import { Button } from '@/components/ui/button';
import {
  DialogClose,
  DialogContent,
  DialogDescription,
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
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import CurrencyInput from '@/components/ui-custom/CurrencyInput';
import { Account } from '@/data/accounts/account';

import { useAccountEditor } from './useAccountEditor';

// This is for the case when the user leaves the number cell empty when the form is submitted.
const currencySchema = z
  .number({
    required_error: 'This field is required',
    invalid_type_error: 'Please enter a valid number',
  })
  .min(0, 'Value must be 0 or greater');

const formSchema = z.object({
  bsb: z.string().regex(/^\d{3}-\d{3}$/, 'BSB must be in the format XXX-XXX'),
  number: z.string().min(1),
  description: z.string().min(1),
  balance: currencySchema,
  reserved: currencySchema,
});

type FormSchema = z.infer<typeof formSchema>;

type EditAccountDialogProps = {
  account: Account;
};

export function EditAccountDialog({ account }: EditAccountDialogProps) {
  const closeRef = useRef<HTMLButtonElement>(null);
  const updateMutation = useUpdateAccountMutation();

  const form = useForm<FormSchema>({
    resolver: zodResolver(formSchema),
  });

  const { resetToOriginal } = useAccountEditor(form, account);

  const onSubmit = async (values: FormSchema) => {
    const controller = new AbortController();
    try {
      const updatedAccount: Account = {
        ...account,
        ...values,
      };
      await updateMutation.mutateAsync({
        account: updatedAccount,
        signal: controller.signal,
      });
      closeRef.current?.click();
    } finally {
      controller.abort();
    }
  };

  return (
    <DialogContent
      className="sm:max-w-[425px] animate-in fade-in-0 zoom-in-99 duration-300 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-90"
      modal
    >
      <DialogHeader>
        <DialogTitle>Edit Account</DialogTitle>
        {/* DialogDescription is required to remove warnings from the console: Missing Description or aria-describedby={undefined} */}
        <DialogDescription className="sr-only"></DialogDescription>
        <Separator className="mb-4 bg-border" />
      </DialogHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="bsb"
            render={({
              field,
            }: {
              field: ControllerRenderProps<FormSchema, 'bsb'>;
            }) => (
              <FormItem>
                <FormLabel htmlFor="bsb-input">BSB</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    id="bsb-input"
                    placeholder="Enter the Account BSB"
                    aria-description="Enter BSB in the format XXX-XXX"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="number"
            render={({
              field,
            }: {
              field: ControllerRenderProps<FormSchema, 'number'>;
            }) => (
              <FormItem>
                <FormLabel htmlFor="account-number-input">
                  Account Number
                </FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    id="account-number-input"
                    placeholder="Enter the Account Number"
                    aria-description="Your bank account number"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="description"
            render={({
              field,
            }: {
              field: ControllerRenderProps<FormSchema, 'description'>;
            }) => (
              <FormItem>
                <FormLabel htmlFor="description-input">Description</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    id="description-input"
                    placeholder="Enter a description"
                    aria-description="A memorable name for this account"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="balance"
            render={({
              field,
            }: {
              field: ControllerRenderProps<FormSchema, 'balance'>;
            }) => (
              <FormItem>
                <FormLabel htmlFor="balance-input">Balance</FormLabel>
                <FormControl>
                  <CurrencyInput
                    {...field}
                    id="balance-input"
                    placeholder="Enter the Account balance"
                    aria-description="Current account balance"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="reserved"
            render={({
              field,
            }: {
              field: ControllerRenderProps<FormSchema, 'reserved'>;
            }) => (
              <FormItem>
                <FormLabel htmlFor="reserved-input">Reserved</FormLabel>
                <FormControl>
                  <CurrencyInput
                    {...field}
                    id="reserved-input"
                    placeholder="Enter the reserved amount"
                    aria-description="Amount reserved for unexpected expenses"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <div className="flex justify-end space-x-2">
            <DialogClose asChild>
              <Button
                variant="outline"
                type="button"
                className="w-24"
                aria-label="Cancel editing account"
                onClick={resetToOriginal}
              >
                Cancel
              </Button>
            </DialogClose>
            <Button
              type="submit"
              className="w-24"
              disabled={!form.formState.isDirty}
              aria-label="Save account changes"
            >
              Save
            </Button>
          </div>
          <DialogClose ref={closeRef} className="hidden" />
        </form>
      </Form>
    </DialogContent>
  );
}
