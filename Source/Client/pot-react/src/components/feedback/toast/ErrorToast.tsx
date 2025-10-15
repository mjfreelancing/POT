import type { LucideIcon } from 'lucide-react';

import { IconToast } from './IconToast';

type ErrorToastProps = {
  icon: LucideIcon;
  title: string;
  description: string;
};

function ErrorToast({ icon, title, description }: ErrorToastProps) {
  return (
    <IconToast
      icon={icon}
      iconColor="text-red-600"
      title={title}
      description={description}
    />
  );
}

export { ErrorToast };
