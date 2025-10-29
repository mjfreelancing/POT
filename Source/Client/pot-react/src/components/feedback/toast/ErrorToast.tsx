import { AlertTriangle } from 'lucide-react';

import { IconToast } from './IconToast';

type ErrorToastProps = {
  title: string;
  description: string;
};

function ErrorToast({ title, description }: ErrorToastProps) {
  return (
    <IconToast
      icon={AlertTriangle}
      iconColor="text-red-600"
      title={title}
      description={description}
    />
  );
}

export { ErrorToast };
