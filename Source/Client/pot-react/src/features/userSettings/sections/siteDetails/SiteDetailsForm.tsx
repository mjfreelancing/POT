import { zodResolver } from '@hookform/resolvers/zod';
import { Building2 } from 'lucide-react';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import { useApiUpdateSite } from '@/api/hooks/useSite';
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
import { Textarea } from '@/components/ui/textarea';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import { useUserStore } from '@/stores';

import type { SiteDetailsFields } from './siteDetailsSchema';
import { siteDetailsSchema } from './siteDetailsSchema';

type SiteDetailsFormProps = {
  readonly?: boolean;
};

function SiteDetailsForm({ readonly = false }: SiteDetailsFormProps) {
  const { userInfo, setUserInfo } = useUserStore();
  const updateSite = useApiUpdateSite();
  const { error, setError } = useErrorContext();

  // Will not be null once the user has logged in - can't get here until then
  const userDetails = userInfo!;
  const siteDetails = userDetails.site;

  const form = useForm<SiteDetailsFields>({
    resolver: zodResolver(siteDetailsSchema),
    defaultValues: {
      name: siteDetails.name,
      description: siteDetails.description || '',
    },
    mode: 'onSubmit',
  });

  useEffect(() => {
    logger.info('SiteDetailsForm', 'Mounted');

    return () => {
      logger.info('SiteDetailsForm', 'Unmounted');
    };
  }, []);

  useEffect(() => {
    form.reset({
      name: siteDetails.name,
      description: siteDetails.description || '',
    });
  }, [siteDetails, form]);

  async function onSubmit(values: SiteDetailsFields) {
    // Clear any previous errors
    setError(null);

    const result = await updateSite.mutateAsync({
      id: siteDetails.rowId,
      data: {
        name: values.name,
        description: values.description,
        etag: siteDetails.etag,
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
            icon={Building2}
            title="Site Details Updated"
            description="Your site details were updated successfully."
          />
        ),
        { duration: 5000 },
      );

      form.reset();
    }

    setUserInfo({
      ...userDetails,
      site: {
        ...siteDetails,
        etag: result.value.etag,
        name: values.name,
        description: values.description,
      },
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
          name="name"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="site-name">Site Name</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  id="site-name"
                  type="text"
                  disabled={readonly}
                  aria-description="The name of your financial management site"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem className="space-y-1">
              <FormLabel htmlFor="site-description">Description</FormLabel>
              <FormControl>
                <Textarea
                  {...field}
                  id="site-description"
                  rows={3}
                  disabled={readonly}
                  aria-description="Optional description for your site"
                  placeholder="Enter a description for your site (optional)"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full mt-2" disabled={readonly}>
          {readonly ? 'View Site Details' : 'Update Site Details'}
        </Button>
      </form>
    </Form>
  );
}

export default SiteDetailsForm;
