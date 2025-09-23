import { JSX } from 'react';

import { useMe } from '@/api/hooks/useMe';

function AccountUserInfo(): JSX.Element {
  const { data, isLoading, isError } = useMe();

  if (isLoading) {
    return (
      <div
        className="mb-6 px-4 py-3 bg-muted rounded-lg animate-pulse h-20"
        aria-busy="true"
      />
    );
  }

  if (isError || !data || !data.success) {
    return (
      <div className="mb-6 px-4 py-3 bg-destructive/10 text-destructive rounded-lg">
        Unable to load user info
      </div>
    );
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
