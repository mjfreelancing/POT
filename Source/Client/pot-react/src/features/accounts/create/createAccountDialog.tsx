import { zodResolver } from '@hookform/resolvers/zod';
import { useRef, useState } from 'react';
import { ControllerRenderProps, useForm } from 'react-hook-form';
import { z } from 'zod';

import { ErrorDialog, ErrorDialogState } from '@/components/dialog/errorDialog';
import MoneyValueInput from '@/components/input/MoneyValueInput';
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
import { CreateAccount } from '@/data/accounts/account';

import { useCreateAccount } from './hooks/useCreateAccount';

const MoneyValueSchema = z
  .number({
    required_error: 'This field is required',
  })
  .min(0, 'Value must be 0 or greater');

const formSchema = z.object({
  bsb: z.string().regex(/^\d{3}-\d{3}$/, 'BSB must be in the format XXX-XXX'),
  number: z.string().min(1),
  description: z.string().min(1),
  balance: MoneyValueSchema,
  reserved: MoneyValueSchema,
});

type FormSchema = z.infer<typeof formSchema>;

export function CreateAccountDialog() {
  const [dialogError, setDialogError] = useState<ErrorDialogState | null>(null);
  const closeRef = useRef<HTMLButtonElement>(null);
  const { createAccount } = useCreateAccount();

  const form = useForm<FormSchema>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      bsb: '',
      number: '',
      description: '',
      balance: 0,
      reserved: 0,
    },
  });

  const onSubmit = async (values: FormSchema) => {
    const account: CreateAccount = {
      bsb: values.bsb,
      number: values.number,
      description: values.description,
      balance: values.balance,
      reserved: values.reserved,
    };

    const result = await createAccount(account);

    if (result.success) {
      closeRef.current?.click();
      form.reset();
    } else {
      setDialogError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  // TODO: Issue - when the error dialog is shown, the following error is in the console (related to this component):
  //
  // Blocked aria-hidden on an element because its descendant retained focus. The focus must not be hidden from assistive technology users.
  //  Avoid using aria-hidden on a focused element or its ancestor. Consider using the inert attribute instead, which will also prevent focus.
  //  For more details, see the aria-hidden section of the WAI-ARIA specification at https://w3c.github.io/aria/#aria-hidden.
  //
  // Tried using these, but it makes no difference:
  //   modal={dialogError === null}
  //   aria-hidden={dialogError !== null}
  //
  // https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Reference/Attributes/aria-hidden
  // Indicates not to apply aria-hidden to focusable elements.

  return (
    <>
      <DialogContent
        className="sm:max-w-[425px] animate-in fade-in-0 zoom-in-99 duration-300 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-90"
        modal
      >
        <DialogHeader>
          <DialogTitle>Create Account</DialogTitle>
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
                    <MoneyValueInput
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
                    <MoneyValueInput
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
                  aria-label="Cancel creating account"
                  onClick={() => form.reset()}
                >
                  Cancel
                </Button>
              </DialogClose>
              <Button
                type="submit"
                className="w-24"
                aria-label="Create account"
              >
                Create
              </Button>
            </div>
            <DialogClose ref={closeRef} className="hidden" />
          </form>
        </Form>
      </DialogContent>

      {/* Using conditional rendering rather than the 'opne' prop so we don't need to perform null checks, such as title={dialogError?.title ?? ''} */}
      {dialogError && (
        <ErrorDialog
          open={true}
          title={dialogError.title}
          description={dialogError.description}
          onOk={() => setDialogError(null)}
        />
      )}
    </>
  );
}
