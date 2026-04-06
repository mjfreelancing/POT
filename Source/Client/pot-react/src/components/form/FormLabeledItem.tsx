import { FormControl, FormItem, FormLabel, FormMessage } from '@/components/ui/form';

type FormLabeledItemProps = {
  label: string;
  htmlFor: string;
  children: React.ReactNode;
  className?: string;
};

function FormLabeledItem({
  label,
  htmlFor,
  children,
  className,
}: FormLabeledItemProps) {
  const itemClassName = className ? `space-y-1 ${className}` : 'space-y-1';

  return (
    <FormItem className={itemClassName}>
      <FormLabel htmlFor={htmlFor}>{label}</FormLabel>
      <FormControl>{children}</FormControl>
      <FormMessage />
    </FormItem>
  );
}

export default FormLabeledItem;
