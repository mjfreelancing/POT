import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { useRef } from 'react';
import { ControllerRenderProps, useForm } from 'react-hook-form';
import { z } from 'zod';

import { useCreateAccount } from '@/api/accounts/hooks/useAccounts';
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
  const closeRef = useRef<HTMLButtonElement>(null);
  const createMutation = useCreateAccount();
  const queryClient = useQueryClient();

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
    const controller = new AbortController();

    try {
      const account: CreateAccount = {
        bsb: values.bsb,
        number: values.number,
        description: values.description,
        balance: values.balance,
        reserved: values.reserved,
      };

      await createMutation.mutateAsync({
        data: account,
        signal: controller.signal,
      });

      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      closeRef.current?.click();
      form.reset();
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
        <DialogTitle>Create Account</DialogTitle>
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
            <Button type="submit" className="w-24" aria-label="Create account">
              Create
            </Button>
          </div>
          <DialogClose ref={closeRef} className="hidden" />
        </form>
      </Form>
    </DialogContent>
  );
}
