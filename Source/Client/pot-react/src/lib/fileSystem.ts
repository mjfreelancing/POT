// File type for the file picker
type FilePickerAcceptType = {
  description: string;
  accept: Record<string, string[]>;
};

// Options for saving files
type SaveFilePickerOptions = {
  suggestedName: string;
  types: FilePickerAcceptType[];
};

// Options for opening files
type OpenFilePickerOptions = {
  types: FilePickerAcceptType[];
  multiple: boolean;
};

// Window extension for saving files
type WindowSaveFile = Window & {
  showSaveFilePicker: (
    options: SaveFilePickerOptions,
  ) => Promise<FileSystemFileHandle>;
};

// Window extension for opening files
type WindowOpenFile = Window & {
  showOpenFilePicker: (
    options: OpenFilePickerOptions,
  ) => Promise<FileSystemFileHandle[]>;
};

export type {
  FilePickerAcceptType,
  OpenFilePickerOptions,
  SaveFilePickerOptions,
  WindowOpenFile,
  WindowSaveFile,
};
