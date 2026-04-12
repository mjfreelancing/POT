type RegisterSWOptions = {
  onNeedRefresh?: () => void;
  onOfflineReady?: () => void;
  onRegisterError?: (error: unknown) => void;
};

function registerSW(options?: RegisterSWOptions) {
  if (options?.onOfflineReady) {
    options.onOfflineReady();
  }

  return function unregister() {
    // no-op in tests
  };
}

export { registerSW };
