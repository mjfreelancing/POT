import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

type SheetShellProps = {
  title: string;
  children: React.ReactNode;
};

type ErrorShape = {
  code?: string;
  description?: string;
};

type ResultWithOptionalError =
  | {
      error?: ErrorShape;
    }
  | null
  | undefined;

type CreateSheetLoadingStateProps = {
  SheetShell: React.ComponentType<SheetShellProps>;
  title: string;
};

type ApiErrorSheetStateProps = {
  result?: ResultWithOptionalError;
  fallbackTitle: string;
  fallbackDescription: string;
  onDismiss: () => void;
};

function CreateSheetLoadingState({
  SheetShell,
  title,
}: CreateSheetLoadingStateProps): React.JSX.Element {
  return (
    <SheetShell title={title}>
      <LoadingMessage isLoading={true} />
    </SheetShell>
  );
}

function ApiErrorSheetState({
  result,
  fallbackTitle,
  fallbackDescription,
  onDismiss,
}: ApiErrorSheetStateProps): React.JSX.Element {
  return (
    <ErrorSheet
      title={result?.error?.code || fallbackTitle}
      description={result?.error?.description || fallbackDescription}
      onDismiss={onDismiss}
    />
  );
}

export { ApiErrorSheetState, CreateSheetLoadingState };
