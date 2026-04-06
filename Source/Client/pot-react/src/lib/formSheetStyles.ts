const FORM_SHEET_STYLES = {
  SHEET_CONTENT:
    'p-6 sm:max-w-lg [&>button:first-of-type]:hidden overflow-y-auto',
  SHEET_INNER: 'space-y-6 pr-6 pl-6',
  ACTION_SECTION: 'space-y-4 pt-2',
  ACTION_ROW: 'flex justify-end space-x-4',
  ACTION_BUTTON_WIDTH: 'w-24',
  DATE_ROW: 'flex flex-col gap-0 sm:flex-row sm:items-center sm:gap-2',
  DATE_TRIGGER: 'w-full min-w-0 sm:flex-1',
  DATE_ACTION_ROW:
    '-mt-0.5 flex w-full justify-end sm:mt-0 sm:w-auto sm:flex-none',
  DATE_ACTION_BUTTON: 'sm:w-16',
} as const;

export { FORM_SHEET_STYLES };
