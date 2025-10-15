import { useEffect, useState } from 'react';

import { useRequestPasswordReset, useVerifyPasswordReset } from '@/api/hooks';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { logger } from '@/lib';

import OtpVerificationForm from './components/OtpVerificationForm';
import SuccessMessage from './components/SuccessMessage';
import UsernameInputForm from './components/UsernameInputForm';
import usePasswordResetFlow from './hooks/usePasswordResetFlow';
import type {
  OtpVerificationStatus,
  PasswordResetDialogProps,
} from './types/passwordResetTypes';

function PasswordResetDialog({
  open,
  onOpenChange,
  onError,
}: PasswordResetDialogProps) {
  const { sendPasswordReset, isPending: isSending } = useRequestPasswordReset();
  const { verifyPasswordReset, isPending: isVerifying } =
    useVerifyPasswordReset();
  const [verificationMessage, setVerificationMessage] = useState<string>('');
  const [verificationStatus, setVerificationStatus] = useState<
    OtpVerificationStatus | undefined
  >();
  const [retryMinutes, setRetryMinutes] = useState<number | undefined>();
  const [hasActiveCountdown, setHasActiveCountdown] = useState(false);
  const {
    state,
    data,
    updateUsername,
    updateReferenceCode,
    updateOtpCode,
    goToOtpVerification,
    goToSuccess,
    goBackToUsername,
    reset,
  } = usePasswordResetFlow();

  // Helper function to clear all error states
  const clearErrorStates = () => {
    setVerificationMessage('');
    setVerificationStatus(undefined);
    setRetryMinutes(undefined);
  };

  // Log dialog mount/unmount for analytics
  useEffect(() => {
    if (open) {
      logger.info('PasswordResetDialog', 'Mounted');
    } else {
      // Only reset if there was a rate limit - otherwise preserve state for accidental closes
      if (verificationStatus === 'TooManyAttempts') {
        clearErrorStates();
        setHasActiveCountdown(false);
        onError(null);
        reset();
      } else {
        // Just clear any error messages but keep the flow state
        clearErrorStates();
        setHasActiveCountdown(false);
        onError(null);
      }
    }
    return () => {
      if (open) {
        logger.info('PasswordResetDialog', 'Unmounted');
      }
    };
  }, [open, onError, reset, verificationStatus]);

  const handleUsernameSubmit = async (username: string) => {
    updateUsername(username);

    // Clear any previous errors when user tries again
    onError(null);

    const result = await sendPasswordReset(username);

    if (result.success) {
      // Store reference code and move to OTP verification
      updateReferenceCode(result.value.referenceCode);
      goToOtpVerification();
    } else {
      logger.error(
        'PasswordResetDialog',
        'Password reset request failed',
        result.error,
      );
      onError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  const handleOtpSubmit = async (verificationCode: string) => {
    updateOtpCode(verificationCode);
    setVerificationMessage(''); // Clear previous errors

    const result = await verifyPasswordReset(
      data.username,
      data.referenceCode,
      verificationCode,
    );

    if (result.success) {
      if (result.value.status === 'Success') {
        goToSuccess();
      } else {
        // Handle different failure scenarios
        setVerificationMessage(result.value.message);
        setVerificationStatus(result.value.status);

        // Clear the failed verification code from state
        updateOtpCode('');

        // Handle cooldown for too many attempts
        if (
          result.value.status === 'TooManyAttempts' &&
          result.value.retryMinutes
        ) {
          // Clear reference code too - user needs fresh OTP session after cooldown
          updateReferenceCode('');
          setRetryMinutes(result.value.retryMinutes);
        }
      }
    } else {
      logger.error(
        'PasswordResetDialog',
        'OTP verification API error',
        result.error,
      );
      onError({
        title: 'Verification Failed',
        description: result.error.description,
      });
    }
  };

  const handleResendCode = async () => {
    // Clear any existing verification error and cooldown when resending
    clearErrorStates();

    // Clear any previous errors when user tries again
    onError(null);

    const result = await sendPasswordReset(data.username);

    if (result.success) {
      // Update the reference code if it changed
      if (result.value.referenceCode) {
        updateReferenceCode(result.value.referenceCode);
      }
    } else {
      logger.error(
        'PasswordResetDialog',
        'Failed to resend code',
        result.error,
      );
      onError({
        title: 'Resend Failed',
        description: result.error.description,
      });
    }
  };

  const handleGoBackToUsername = () => {
    // Clear all error states when going back
    clearErrorStates();
    // Clear parent error as well
    onError(null);
    // Use flow hook to go back and clear flow data
    goBackToUsername();
  };

  const handleStartOver = () => {
    // Clear all dialog state and go back to username input
    clearErrorStates();
    // Clear parent error when starting over
    onError(null);
    reset();
  };

  const handleSuccess = () => {
    // Clear any errors - flow completed successfully
    onError(null);
    reset();
    onOpenChange(false);
  };

  const getDialogTitle = () => {
    switch (state) {
      case 'username-input':
        return 'Reset Password';
      case 'otp-verification':
        return 'Enter Verification Code';
      case 'success':
        return 'Password Reset Complete';
      default:
        return 'Reset Password';
    }
  };

  const getDialogDescription = () => {
    switch (state) {
      case 'username-input':
        return 'Enter your username to receive a reset code';
      case 'otp-verification':
        return 'Enter the verification code we sent you';
      case 'success':
        return ''; // No description needed - green checkmark says it all
      default:
        return '';
    }
  };

  const showStartOverButton = state !== 'username-input' && state !== 'success';
  const isStartOverDisabled = hasActiveCountdown;

  const renderCurrentPhase = () => {
    switch (state) {
      case 'username-input':
        return (
          <UsernameInputForm
            onSubmit={handleUsernameSubmit}
            isLoading={isSending}
          />
        );
      case 'otp-verification':
        return (
          <OtpVerificationForm
            username={data.username}
            referenceCode={data.referenceCode}
            onSubmit={handleOtpSubmit}
            onResendCode={handleResendCode}
            onGoBack={handleGoBackToUsername}
            onCountdownChange={setHasActiveCountdown}
            isLoading={isVerifying}
            isResending={isSending}
            verificationMessage={verificationMessage}
            verificationStatus={verificationStatus}
            retryMinutes={retryMinutes}
          />
        );
      case 'success':
        return <SuccessMessage onComplete={handleSuccess} />;
      default:
        return null;
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm top-[30%] translate-y-[-30%] left-[60%] translate-x-[-40%] flex flex-col">
        <DialogHeader>
          <DialogTitle className={state === 'success' ? 'text-center' : ''}>
            {getDialogTitle()}
          </DialogTitle>
          <DialogDescription>{getDialogDescription()}</DialogDescription>
        </DialogHeader>

        <div className="mt-6">{renderCurrentPhase()}</div>

        {showStartOverButton && (
          <DialogFooter className="pt-4 border-t border-border/50">
            <button
              type="button"
              onClick={handleStartOver}
              disabled={isStartOverDisabled}
              className="text-sm text-muted-foreground hover:text-primary underline-offset-4 hover:underline transition-colors disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:no-underline"
            >
              Start over
            </button>
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  );
}

export default PasswordResetDialog;
