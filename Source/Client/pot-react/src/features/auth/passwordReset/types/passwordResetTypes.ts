import type { DisplayError } from '@/lib';

/**
 * Password reset dialog flow states
 */
type PasswordResetState =
  | 'username-input' // Phase 1: Enter username
  | 'otp-verification' // Phase 2: Enter OTP + go back / resend options
  | 'success'; // Phase 3: Success message with login instructions

/**
 * Data collected throughout the password reset flow
 */
type PasswordResetData = {
  username: string;
  referenceCode: string;
  otpCode?: string;
};

/**
 * Props for the main password reset dialog
 */
type PasswordResetDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onError: (error: DisplayError | null) => void; // null to clear errors
};

export type {
  DisplayError,
  PasswordResetData,
  PasswordResetDialogProps,
  PasswordResetState,
};
