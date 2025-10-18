import { useCallback, useState } from 'react';

import type {
  PasswordResetData,
  PasswordResetState,
} from '../types/passwordResetTypes';

function usePasswordResetFlow() {
  const [state, setState] = useState<PasswordResetState>('username-input');
  const [data, setData] = useState<PasswordResetData>({
    username: '',
    referenceCode: '',
  });

  const updateUsername = useCallback((username: string) => {
    setData(prev => ({ ...prev, username }));
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
    setState('username-input');
    setData({ username: '', referenceCode: '' });
  }, []);

  return {
    state,
    data,
    updateUsername,
    updateReferenceCode,
    updateOtpCode,
    goToOtpVerification,
    goToSuccess,
    reset,
  };
}

export default usePasswordResetFlow;
