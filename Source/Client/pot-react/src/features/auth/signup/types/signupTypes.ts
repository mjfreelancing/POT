import type { DisplayError } from '@/lib';

/**
 * Signup dialog flow states
 */
type SignupState =
  | 'user-input' // Phase 1: Enter username and email
  | 'otp-verification' // Phase 2: Enter OTP codes
  | 'success'; // Phase 3: Success message with login instructions

/**
 * Data collected throughout the signup flow
 */
type SignupData = {
  username: string;
  email: string;
  referenceCode: string;
  otpCode?: string;
};

/**
 * Props for the main signup dialog
 */
type SignupDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onError: (error: DisplayError | null) => void; // null to clear errors
};

export type { DisplayError, SignupData, SignupDialogProps, SignupState };
