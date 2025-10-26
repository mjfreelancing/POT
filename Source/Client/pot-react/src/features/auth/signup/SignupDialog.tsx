import { useEffect, useState } from 'react';

import { useCompleteSignup, useRequestSignup } from '@/api/hooks';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { logger } from '@/lib';

import type { OtpVerificationStatus } from '../shared';
import { OtpVerificationForm } from '../shared';
import { SignupForm, SignupSuccessMessage } from './components';
import useSignupFlow from './hooks/useSignupFlow';
import type { SignupDialogProps } from './types/signupTypes';

function SignupDialog({ open, onOpenChange, onError }: SignupDialogProps) {
  const { sendSignup, isPending: isSending } = useRequestSignup();
  const { completeSignup, isPending: isCompleting } = useCompleteSignup();
  const [signupErrorMessage, setSignupErrorMessage] = useState<string>('');
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
    updateEmail,
    updateReferenceCode,
    updateOtpCode,
    goToOtpVerification,
    goToSuccess,
    reset,
  } = useSignupFlow();

  // Helper function to clear all error states
  const clearErrorStates = () => {
    setSignupErrorMessage('');
    setVerificationMessage('');
    setVerificationStatus(undefined);
    setRetryMinutes(undefined);
  };

  // Log dialog mount/unmount for analytics
  useEffect(() => {
    if (open) {
      logger.info('SignupDialog', 'Mounted');
    } else {
      // Reset if there was a rate limit or expired code - otherwise preserve state for accidental closes
      if (
        verificationStatus === 'TooManyAttempts' ||
        verificationStatus === 'Expired'
      ) {
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
        logger.info('SignupDialog', 'Unmounted');
      }
    };
  }, [open, onError, reset, verificationStatus]);

  const handleSignupSubmit = async (username: string, email: string) => {
    updateUsername(username);
    updateEmail(email);

    // Clear any previous errors when user tries again
    setSignupErrorMessage('');
    onError(null);

    const result = await sendSignup(username, email);

    if (result.success) {
      if (result.value.status === 'Success' && result.value.referenceCode) {
        // Store reference code and move to OTP verification
        updateReferenceCode(result.value.referenceCode);
        goToOtpVerification();
      } else if (result.value.status === 'UsernameTaken') {
        // Show username taken error inline in the form
        setSignupErrorMessage(result.value.message);
      }
    } else {
      logger.error('SignupDialog', 'Signup request failed', result.error);
      onError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  const handleOtpSubmit = async (verificationCode: string) => {
    updateOtpCode(verificationCode);
    setVerificationMessage(''); // Clear previous errors

    const result = await completeSignup(
      data.username,
      data.referenceCode,
      verificationCode,
    );

    if (result.success) {
      if (result.value.status === 'Success') {
        goToSuccess();
      } else if (result.value.status === 'UsernameTaken') {
        // Username taken - auto-navigate back to user input immediately
        // Show error inline in the form when we navigate back
        setSignupErrorMessage(result.value.message);
        reset();
      } else {
        // Handle other failure scenarios (InvalidCode, Expired, TooManyAttempts)
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
      logger.error('SignupDialog', 'OTP verification API error', result.error);
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

    const result = await sendSignup(data.username, data.email);

    if (result.success) {
      if (result.value.status === 'Success' && result.value.referenceCode) {
        // Update the reference code if it changed
        updateReferenceCode(result.value.referenceCode);
      }
    } else {
      logger.error('SignupDialog', 'Failed to resend code', result.error);
      onError({
        title: 'Resend Failed',
        description: result.error.description,
      });
    }
  };

  const handleStartOver = () => {
    // Clear all dialog state and go back to user input
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
      case 'user-input':
        return 'Create Account';
      case 'otp-verification':
        return 'Enter Verification Code';
      case 'success':
        return 'Account Created';
      default:
        return 'Sign Up';
    }
  };

  const getDialogDescription = () => {
    switch (state) {
      case 'user-input':
        return 'Enter your username and email to get started';
      case 'otp-verification':
        return 'Enter the verification code we sent you';
      case 'success':
        return ''; // No description needed - green checkmark says it all
      default:
        return '';
    }
  };

  const showStartOverButton = state !== 'user-input' && state !== 'success';
  const isStartOverDisabled = hasActiveCountdown;

  const renderCurrentPhase = () => {
    switch (state) {
      case 'user-input':
        return (
          <SignupForm
            onSubmit={handleSignupSubmit}
            isLoading={isSending}
            errorMessage={signupErrorMessage}
          />
        );
      case 'otp-verification':
        return (
          <OtpVerificationForm
            username={data.username}
            referenceCode={data.referenceCode}
            onSubmit={handleOtpSubmit}
            onResendCode={handleResendCode}
            onCountdownChange={setHasActiveCountdown}
            isLoading={isCompleting}
            isResending={isSending}
            verificationMessage={verificationMessage}
            verificationStatus={verificationStatus}
            retryMinutes={retryMinutes}
          />
        );
      case 'success':
        return <SignupSuccessMessage onComplete={handleSuccess} />;
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

export default SignupDialog;
