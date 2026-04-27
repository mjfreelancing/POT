type SettingsSectionFormSubmitResult = 'saved' | 'blocked' | 'invalid';

type SettingsSectionFormHandle = {
  submit: () => Promise<SettingsSectionFormSubmitResult>;
  discard: () => void;
};

type SettingsSectionFormProps = {
  onDirtyChange?: (isDirty: boolean) => void;
};

export type {
  SettingsSectionFormHandle,
  SettingsSectionFormProps,
  SettingsSectionFormSubmitResult,
};
