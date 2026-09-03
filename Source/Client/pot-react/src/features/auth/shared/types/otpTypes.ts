/**
 * OTP verification statuses from API responses
 * Shared between password reset and signup flows
 */
export type OtpVerificationStatus =
  'Success' | 'InvalidCode' | 'Expired' | 'TooManyAttempts';

/**
 * Props for the reusable OTP verification form component
 */
export type OtpVerificationFormProps = {
  username: string;
  referenceCode: string;
  onSubmit: (verificationCode: string) => Promise<void>;
  onResendCode: () => Promise<void>;
  onCountdownChange?: (hasActiveCountdown: boolean) => void;
  isLoading?: boolean;
  isResending?: boolean;
  verificationMessage?: string;
  verificationStatus?: OtpVerificationStatus;
  retryMinutes?: number;
};
