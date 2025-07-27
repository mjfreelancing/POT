import { LucideIcon } from 'lucide-react';

import { IconToast } from './IconToast';

type SuccessToastProps = {
  icon: LucideIcon;
  title: string;
  description: string;
  details?: string;
};

function SuccessToast({
  icon,
  title,
  description,
  details,
}: SuccessToastProps) {
  return (
    <IconToast
      icon={icon}
      iconColor="text-green-600"
      title={title}
      description={description}
      details={details}
    />
  );
}

export { SuccessToast };
