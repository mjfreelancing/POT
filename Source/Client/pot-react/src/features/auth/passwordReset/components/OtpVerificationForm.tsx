import { useEffect, useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  InputOTP,
  InputOTPGroup,
  InputOTPSlot,
} from '@/components/ui/input-otp';
import { Label } from '@/components/ui/label';

import type { OtpVerificationStatus } from '../types/passwordResetTypes';

const defaultCountdownSeconds = 60; // seconds

type OtpVerificationFormProps = {
  username: string;
  referenceCode: string;
  onSubmit: (verificationCode: string) => Promise<void>;
  onResendCode: () => Promise<void>;
  onGoBack: () => void;
  onCountdownChange?: (hasActiveCountdown: boolean) => void; // Notify parent of countdown state
  isLoading?: boolean;
  isResending?: boolean;
  verificationMessage?: string;
  verificationStatus?: OtpVerificationStatus; // API status for logic decisions
  retryMinutes?: number; // From TooManyAttempts API response
};

function OtpVerificationForm({
  username,
  referenceCode,
  onSubmit,
  onResendCode,
  onGoBack,
  onCountdownChange,
  isLoading = false,
  isResending = false,
  verificationMessage,
  verificationStatus,
  retryMinutes,
}: OtpVerificationFormProps) {
  const [verificationCode, setVerificationCode] = useState('');
  const [resendCountdown, setResendCountdown] = useState(
    defaultCountdownSeconds,
  );
  const [canResend, setCanResend] = useState(false);

  // Update countdown when retryMinutes changes (from TooManyAttempts)
  useEffect(() => {
    if (retryMinutes && retryMinutes > 0) {
      // Convert minutes to seconds
      setResendCountdown(retryMinutes * 60);
      setCanResend(false);
    }
  }, [retryMinutes]);

  // Force countdown reset when status is TooManyAttempts
  useEffect(() => {
    if (verificationStatus === 'TooManyAttempts' && retryMinutes) {
      // Convert minutes to seconds
      setResendCountdown(retryMinutes * 60);
      setCanResend(false);
    }
  }, [verificationStatus, retryMinutes]);

  // Clear verification code when an error occurs
  useEffect(() => {
    if (verificationMessage) {
      setVerificationCode('');
    }
  }, [verificationMessage]);

  // Countdown timer for resend
  useEffect(() => {
    if (resendCountdown > 0) {
      const timer = setTimeout(() => {
        setResendCountdown(prev => prev - 1);
      }, 1000);
      return () => clearTimeout(timer);
    } else {
      setCanResend(true);
    }
  }, [resendCountdown]);

  // Notify parent when countdown state changes
  useEffect(() => {
    if (onCountdownChange) {
      onCountdownChange(!canResend);
    }
  }, [canResend, onCountdownChange]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (verificationCode.length === 6) {
      await onSubmit(verificationCode);
    }
  };

  const handleResend = async () => {
    if (canResend) {
      await onResendCode();
      // Reset countdown
      setResendCountdown(defaultCountdownSeconds);
      setCanResend(false);
      setVerificationCode('');
    }
  };

  const handleVerificationCodeChange = (value: string) => {
    setVerificationCode(value);
    // Remove auto-submit - let user click "Verify Code" button
  };

  // Format countdown time as mm:ss for better UX
  const formatCountdown = (seconds: number): string => {
    if (seconds <= 60) {
      return `${seconds} secs`;
    }

    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')} mins`;
  };

  return (
    <div className="space-y-6 w-full">
      <form onSubmit={handleSubmit} className="space-y-6 w-full">
        <div className="space-y-6 w-full">
          {/* Reference Code - Read Only */}
          <div className="space-y-3">
            <Label
              htmlFor="reference-code"
              className="text-sm font-medium text-muted-foreground"
            >
              Reference Code
            </Label>
            <div className="flex justify-center">
              <div className="p-2 bg-muted/50 rounded-lg border border-muted">
                <InputOTP maxLength={6} value={referenceCode} disabled={true}>
                  <InputOTPGroup className="gap-2">
                    <InputOTPSlot
                      index={0}
                      className="w-12 h-12 text-lg font-semibold"
                    />
                    <InputOTPSlot
                      index={1}
                      className="w-12 h-12 text-lg font-semibold"
                    />
                    <InputOTPSlot
                      index={2}
                      className="w-12 h-12 text-lg font-semibold"
                    />
                    <InputOTPSlot
                      index={3}
                      className="w-12 h-12 text-lg font-semibold"
                    />
                    <InputOTPSlot
                      index={4}
                      className="w-12 h-12 text-lg font-semibold"
                    />
                    <InputOTPSlot
                      index={5}
                      className="w-12 h-12 text-lg font-semibold"
                    />
                  </InputOTPGroup>
                </InputOTP>
              </div>
            </div>
          </div>

          {/* Verification Code - User Input */}
          <div className="space-y-3">
            <Label
              htmlFor="verification-code"
              className="text-sm font-semibold"
            >
              Verification Code
            </Label>
            <div className="flex justify-center">
              <div className="p-2 border-2 border-dashed border-primary/30 rounded-lg bg-background">
                <InputOTP
                  maxLength={6}
                  value={verificationCode}
                  onChange={handleVerificationCodeChange}
                  disabled={isLoading}
                >
                  <InputOTPGroup className="gap-2">
                    <InputOTPSlot
                      index={0}
                      className="w-12 h-12 text-lg font-bold"
                    />
                    <InputOTPSlot
                      index={1}
                      className="w-12 h-12 text-lg font-bold"
                    />
                    <InputOTPSlot
                      index={2}
                      className="w-12 h-12 text-lg font-bold"
                    />
                    <InputOTPSlot
                      index={3}
                      className="w-12 h-12 text-lg font-bold"
                    />
                    <InputOTPSlot
                      index={4}
                      className="w-12 h-12 text-lg font-bold"
                    />
                    <InputOTPSlot
                      index={5}
                      className="w-12 h-12 text-lg font-bold"
                    />
                  </InputOTPGroup>
                </InputOTP>
              </div>
            </div>
            {verificationMessage && (
              <div className="bg-red-500/15 border border-red-400/30 rounded-md p-4 text-center my-4">
                <p className="text-sm font-semibold text-red-400">
                  {verificationMessage}
                </p>
                {verificationStatus === 'TooManyAttempts' &&
                  retryMinutes &&
                  retryMinutes > 0 && (
                    <p className="text-xs text-red-300 mt-1 font-medium">
                      Please wait before trying again
                    </p>
                  )}
              </div>
            )}
            <p className="text-sm text-muted-foreground text-center">
              Code sent to the email for{' '}
              <span className="font-semibold text-foreground">{username}</span>
            </p>
          </div>
        </div>

        <Button
          type="submit"
          className="w-full"
          disabled={
            isLoading ||
            verificationCode.length !== 6 ||
            !referenceCode ||
            referenceCode.length !== 6
          }
        >
          {isLoading
            ? 'Verifying...'
            : !referenceCode || referenceCode.length !== 6
              ? 'Request Code First'
              : 'Verify Code'}
        </Button>
      </form>

      {/* Resend and Go Back Options */}
      <div className="space-y-3">
        <div className="flex justify-center">
          <Button
            type="button"
            variant="ghost"
            onClick={handleResend}
            disabled={!canResend || isResending}
            className="text-sm"
          >
            {isResending
              ? 'Sending...'
              : canResend
                ? 'Resend Code'
                : `Resend in ${formatCountdown(resendCountdown)}`}
          </Button>
        </div>

        <div className="text-center">
          <button
            type="button"
            onClick={onGoBack}
            disabled={!canResend}
            className="text-sm text-muted-foreground hover:text-primary underline-offset-4 hover:underline transition-colors disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:no-underline"
          >
            Go back to username
          </button>
        </div>
      </div>
    </div>
  );
}

export default OtpVerificationForm;
