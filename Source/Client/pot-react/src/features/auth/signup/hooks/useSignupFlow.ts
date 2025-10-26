import { useCallback, useState } from 'react';

import type { SignupData, SignupState } from '../types/signupTypes';

function useSignupFlow() {
  const [state, setState] = useState<SignupState>('user-input');
  const [data, setData] = useState<SignupData>({
    username: '',
    email: '',
    referenceCode: '',
  });

  const updateUsername = useCallback((username: string) => {
    setData(prev => ({ ...prev, username }));
  }, []);

  const updateEmail = useCallback((email: string) => {
    setData(prev => ({ ...prev, email }));
  }, []);

  const updateReferenceCode = useCallback((referenceCode: string) => {
    setData(prev => ({ ...prev, referenceCode }));
  }, []);

  const updateOtpCode = useCallback((otpCode: string) => {
    setData(prev => ({ ...prev, otpCode }));
  }, []);

  const goToOtpVerification = useCallback(() => {
    setState('otp-verification');
  }, []);

  const goToSuccess = useCallback(() => {
    setState('success');
  }, []);

  const reset = useCallback(() => {
    setState('user-input');
    setData({ username: '', email: '', referenceCode: '' });
  }, []);

  return {
    state,
    data,
    updateUsername,
    updateEmail,
    updateReferenceCode,
    updateOtpCode,
    goToOtpVerification,
    goToSuccess,
    reset,
  };
}

export default useSignupFlow;
