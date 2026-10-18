import { useRef, useState } from 'react';

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib';

/** A value the caller has to type before the destructive action is enabled. */
type ConfirmationField = {
  id: string;
  label: string;
  expectedValue: string;
  placeholder?: string;
  helperText?: string;
};

/** The values the caller entered, keyed by confirmation field id. */
type DestructiveConfirmValues = {
  fields: Record<string, string>;
  password: string;
};

type DestructiveConfirmDialogProps = {
  open: boolean;
  title: string;
  description: React.ReactNode;
  confirmLabel: string;
  fields?: ConfirmationField[];
  /** Renders a required password field when a label is supplied. */
  passwordLabel?: string;
  passwordHelperText?: string;
  /** A refusal from the last attempt, shown in place so the dialog can stay open. */
  error?: string;
  busy?: boolean;
  onConfirm: (values: DestructiveConfirmValues) => void;
  onCancel: () => void;
};

function matches(expectedValue: string, enteredValue: string | undefined): boolean {
  return (enteredValue ?? '').trim().toLowerCase() === expectedValue.trim().toLowerCase();
}

/**
 * Browsers and password managers can write into a field without React seeing an event, which would leave a
 * value on screen that the confirmation never registered. The dialog is deliberately typed, so the field is
 * returned to the value the caller actually entered when it gains focus.
 */
function discardInjectedValue(
  event: React.FocusEvent<HTMLInputElement>,
  enteredValue: string,
) {
  if (event.currentTarget.value !== enteredValue) {
    event.currentTarget.value = enteredValue;
  }
}

// Held in a child of the dialog so the entered values start empty every time the dialog opens.
function DestructiveConfirmContent({
  title,
  description,
  confirmLabel,
  fields = [],
  passwordLabel,
  passwordHelperText,
  error,
  busy = false,
  onConfirm,
  onCancel,
}: Omit<DestructiveConfirmDialogProps, 'open'>) {
  const cancelRef = useRef<HTMLButtonElement>(null);
  const [fieldValues, setFieldValues] = useState<Record<string, string>>({});
  const [password, setPassword] = useState('');

  const allFieldsMatch = fields.every(field =>
    matches(field.expectedValue, fieldValues[field.id]),
  );
  const passwordEntered = passwordLabel === undefined || password.length > 0;
  const canConfirm = allFieldsMatch && passwordEntered && !busy;

  function handleConfirm() {
    if (!canConfirm) {
      return;
    }

    onConfirm({ fields: fieldValues, password });
  }

  return (
    <AlertDialogContent
      className="z-[70] max-h-[85dvh] overflow-y-auto"
      onOpenAutoFocus={event => {
        event.preventDefault();
        cancelRef.current?.focus();
      }}
    >
      <AlertDialogHeader className="text-left">
        <AlertDialogTitle>{title}</AlertDialogTitle>
        <AlertDialogDescription asChild>
          <div>{description}</div>
        </AlertDialogDescription>
      </AlertDialogHeader>

      {error && (
        <p
          role="alert"
          className="rounded-md border border-destructive/50 bg-destructive/10 px-3 py-2 text-sm text-destructive-high-contrast"
        >
          {error}
        </p>
      )}

      {(fields.length > 0 || passwordLabel !== undefined) && (
        <div className="space-y-4">
          {fields.map(field => (
            <div key={field.id} className="space-y-1.5">
              <Label htmlFor={field.id}>{field.label}</Label>
              <Input
                id={field.id}
                value={fieldValues[field.id] ?? ''}
                placeholder={field.placeholder}
                autoComplete="off"
                autoCapitalize="none"
                autoCorrect="off"
                spellCheck={false}
                disabled={busy}
                onFocus={event =>
                  discardInjectedValue(event, fieldValues[field.id] ?? '')
                }
                onChange={event =>
                  setFieldValues(current => ({ ...current, [field.id]: event.target.value }))
                }
              />
              {field.helperText && (
                <p className="text-xs text-muted-foreground">{field.helperText}</p>
              )}
            </div>
          ))}

          {passwordLabel !== undefined && (
            <div className="space-y-1.5">
              <Label htmlFor="destructive-confirm-password">{passwordLabel}</Label>
              <Input
                id="destructive-confirm-password"
                type="password"
                // "new-password" is the only hint browsers honour here: the dialog offers no stored
                // credential, so nothing may be filled in on the user's behalf.
                autoComplete="new-password"
                value={password}
                disabled={busy}
                onFocus={event => discardInjectedValue(event, password)}
                onChange={event => setPassword(event.target.value)}
              />
              {passwordHelperText && (
                <p className="text-xs text-muted-foreground">{passwordHelperText}</p>
              )}
            </div>
          )}
        </div>
      )}

      <AlertDialogFooter className={cn('flex-row justify-end gap-2')}>
        <AlertDialogCancel
          ref={cancelRef}
          className="mt-0"
          disabled={busy}
          onClick={onCancel}
        >
          Cancel
        </AlertDialogCancel>
        <AlertDialogAction
          variant="destructive"
          disabled={!canConfirm}
          aria-busy={busy}
          onClick={event => {
            // Keep the dialog open until the caller closes it, so a refusal stays visible.
            event.preventDefault();
            handleConfirm();
          }}
        >
          {confirmLabel}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  );
}

/**
 * Confirmation dialog for a destructive action.
 *
 * The destructive action stays disabled until every confirmation field matches its expected value, compared
 * trimmed and case-insensitively, and the password field (when shown) has been filled in. Cancel is the first
 * action and takes focus when the dialog opens.
 */
function DestructiveConfirmDialog({
  open,
  ...contentProps
}: DestructiveConfirmDialogProps) {
  return (
    <AlertDialog open={open}>
      <DestructiveConfirmContent {...contentProps} />
    </AlertDialog>
  );
}

export default DestructiveConfirmDialog;
export type { ConfirmationField, DestructiveConfirmDialogProps, DestructiveConfirmValues };
