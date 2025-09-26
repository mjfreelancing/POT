import { JSX, useEffect, useState } from 'react';

import { useMe } from '@/api/hooks/useMe';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { Skeleton } from '@/components/ui/skeleton';
import { logger } from '@/lib/logging';

function AccountUserInfo(): JSX.Element {
  const { data, isLoading } = useMe();
  const [error, setError] = useState<null | {
    title: string;
    description: string;
  }>(null);

  useEffect(() => {
    logger.info('AccountUserInfo', 'Mounted');

    return () => {
      logger.info('AccountUserInfo', 'Unmounted');
    };
  }, []);

  if (isLoading) {
    return <Skeleton className="mb-6 px-4 py-3 h-20 w-full rounded-lg" />;
  }

  if (error) {
    return (
      <ErrorSheet
        title={error.title}
        description={error.description}
        onDismiss={() => setError(null)}
      />
    );
  }

  if (data && !data.success) {
    setError({
      title: data.error.code,
      description: data.error.description,
    });

    return <div />;
  }

  const { username, displayName, email } = data.value;

  return (
    <div className="mb-6 px-4 py-3 bg-muted rounded-lg">
      <div className="font-semibold text-primary text-base">{displayName}</div>
      <div className="flex flex-row gap-2 mt-1 items-end">
        <span className="text-xs font-semibold text-primary leading-5">
          Username:
        </span>
        <span className="font-mono text-sm text-muted-foreground leading-5 relative top-[2px]">
          {username}
        </span>
      </div>
      <div className="flex flex-row gap-2 mt-0.5 items-end">
        <span className="text-xs font-semibold text-primary leading-5">
          Email:
        </span>
        <span className="font-mono text-sm text-muted-foreground leading-5 relative top-[2px]">
          {email}
        </span>
      </div>
    </div>
  );
}

export { AccountUserInfo };
